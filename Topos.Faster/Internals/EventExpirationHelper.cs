using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FASTER.core;
using Topos.Extensions;
using Topos.Faster;
using Topos.Logging;
using Topos.Serialization;

namespace Topos.Internals;

class EventExpirationHelper : IDisposable
{
    public static readonly TimeSpan DefaultMaxAge = TimeSpan.FromDays(7.1);

    readonly ConcurrentDictionary<string, Lazy<Task>> _compactionTasks = new();
    readonly CancellationTokenSource _cancellationTokenSource = new();
    readonly ConcurrentDictionary<string, TimeSpan> _maxEventAgePerTopic;
    readonly ILogEntrySerializer _logEntrySerializer;
    readonly IDeviceManager _deviceManager;
    readonly ILogger _logger;

    public EventExpirationHelper(ILoggerFactory loggerFactory, IDeviceManager deviceManager, ILogEntrySerializer logEntrySerializer, IEnumerable<KeyValuePair<string, TimeSpan>> maxAgesPerTopic)
    {
        if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
        if (maxAgesPerTopic == null) throw new ArgumentNullException(nameof(maxAgesPerTopic));
        _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
        _logEntrySerializer = logEntrySerializer ?? throw new ArgumentNullException(nameof(logEntrySerializer));
        _logger = loggerFactory.GetLogger(GetType());
        _maxEventAgePerTopic = new ConcurrentDictionary<string, TimeSpan>(maxAgesPerTopic);
    }

    public TimeSpan CompactionInterval { get; set; } = TimeSpan.FromMinutes(60);

    public void RegisterActivity(string topic)
    {
        // will hit this most of the times
        if (_compactionTasks.ContainsKey(topic)) return;

        var didAddTask = _compactionTasks.TryAdd(topic, new Lazy<Task>(() => PeriodicallyCompactTopic(topic)));

        // if there was a race, don't do anything 
        if (!didAddTask) return;

        Task.Run(() => _compactionTasks[topic].Value);
    }

    async Task PeriodicallyCompactTopic(string topic)
    {
        var token = _cancellationTokenSource.Token;
        var maxAge = GetMaxAgeFor(topic);

        _logger.Debug("Starting compaction task for topic {topic} with max age {maxAge} and interval {interval}",
            topic, maxAge, CompactionInterval);

        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(CompactionInterval, token);

                var log = _deviceManager.GetLog(topic);
                var cutoff = (DateTimeOffset.Now - maxAge).Floor(TimeSpan.FromMinutes(1));
                var beginAddress = log.BeginAddress;

                var truncateUntilAddress = GetTruncateUntilAddress(log, cutoff, beginAddress, token);
                if (truncateUntilAddress <= beginAddress) continue;

                log.TruncateUntil(untilAddress: truncateUntilAddress);

                await log.WaitForCommitAsync(token: token);

                _logger.Debug("Successfully truncated the log from {beginAddress} to {untilAddress}",
                    beginAddress, truncateUntilAddress);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                // it's ok
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unhandled error in compaction task for topic {topic}", topic);
            }
        }

        _logger.Debug("Stopped compaction task for topic {topic}", topic);
    }

    long GetTruncateUntilAddress(FasterLog log, DateTimeOffset cutoff, long beginAddress, CancellationToken token)
    {
        var truncateUntilAddress = beginAddress;

        using var iterator = log.Scan(beginAddress, long.MaxValue);

        while (iterator.GetNext(out var bytes, out var length, out var currentAddress))
        {
            token.ThrowIfCancellationRequested();

            // if it's the dummy data written at the beginning of each log, just skip it
            if (bytes.Length == 3 && bytes.SequenceEqual(FasterLogConsumerImplementation.DummyData))
            {
                continue;
            }

            try
            {
                var entry = _logEntrySerializer.Deserialize(bytes);

                if (!ShouldTruncateToHere(entry, cutoff)) break;

                truncateUntilAddress = currentAddress;
            }
            catch (Exception exception)
            {
                throw new IOException($"An error occurred during scan for a truncate-until address - current address: {currentAddress} - {length} bytes read: {Convert.ToBase64String(bytes)}", exception);
            }
        }

        return truncateUntilAddress;
    }

    static bool ShouldTruncateToHere(TransportMessage entry, DateTimeOffset cutoff)
    {
        if (!entry.Headers.TryGetValue(ToposHeaders.Time, out var timeStr)) return true;

        try
        {
            var time = timeStr.ToDateTimeOffset();

            if (time > cutoff) return false;
        }
        // ReSharper disable once EmptyGeneralCatchClause
        catch
        {
        }

        return true;
    }

    TimeSpan GetMaxAgeFor(string topic) => _maxEventAgePerTopic.TryGetValue(topic, out var result) ? result : DefaultMaxAge;

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        try
        {
            if (!_compactionTasks.Any()) return;

            var compactionTasks = _compactionTasks.Values.Where(t => t.IsValueCreated)
                .Select(t => t.Value)
                .ToList();

            var timeout = TimeSpan.FromSeconds(4);

            if (!Task.WhenAll(compactionTasks).WaitSafe(timeout))
            {
                _logger.Warn("One or more compaction tasks did not exit within timeout {timeout}", timeout);
            }
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _compactionTasks.Clear();
        }
    }
}