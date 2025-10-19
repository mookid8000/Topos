using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Topos.Consumer;
using Topos.Extensions;
using Topos.Internals;
using Topos.Logging;
using Topos.Serialization;
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable 1998

namespace Topos.Faster;

class FasterLogProducerImplementation : IProducerImplementation, IInitializable
{
    readonly CancellationTokenSource _cancellationTokenSource = new();
    readonly ConcurrentQueue<WriteTask> _writeTasks = new();
    readonly AsyncSemaphore _queueItemsSemaphore = new(initialCount: 0);
    readonly EventExpirationHelper _eventExpirationHelper;
    readonly ILogEntrySerializer _logEntrySerializer;
    readonly IDeviceManager _deviceManager;
    readonly ILogger _logger;

    Task _writer;

    public FasterLogProducerImplementation(ILoggerFactory loggerFactory, IDeviceManager deviceManager, ILogEntrySerializer logEntrySerializer, EventExpirationHelper eventExpirationHelper)
    {
        if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
        _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
        _logEntrySerializer = logEntrySerializer ?? throw new ArgumentNullException(nameof(logEntrySerializer));
        _eventExpirationHelper = eventExpirationHelper ?? throw new ArgumentNullException(nameof(eventExpirationHelper));
        _logger = loggerFactory.GetLogger(GetType());
    }

    public Task SendAsync(string topic, string partitionKey, TransportMessage transportMessage, CancellationToken cancellationToken = default)
    {
        var writeTask = new WriteTask(topic, partitionKey, [transportMessage], cancellationToken);

        return EnqueueWriteTask(writeTask);
    }

    public Task SendManyAsync(string topic, string partitionKey, IEnumerable<TransportMessage> transportMessages, CancellationToken cancellationToken = default)
    {
        var writeTask = new WriteTask(topic, partitionKey, transportMessages, cancellationToken);

        return EnqueueWriteTask(writeTask);
    }

    public void Initialize()
    {
        if (_writer != null) throw new InvalidOperationException("Attempted to initialize FasterLogProducerImplementation, but it has been initialized already!");
        _logger.Info("Initializing FasterLog producer");
        _writer = Task.Run(WriterTask);
    }

    Task EnqueueWriteTask(WriteTask writeTask)
    {
        _writeTasks.Enqueue(writeTask);
        _queueItemsSemaphore.Release();
        return writeTask.Task;
    }

    async Task WriterTask()
    {
        var cancellationToken = _cancellationTokenSource.Token;

        _logger.Debug("Starting FasterLog serialized writer task");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await _queueItemsSemaphore.WaitAsync(cancellationToken);

                var tasks = DequeueNext(maxCount: 100);

                _logger.Debug("Got {count} write tasks", tasks.Count);

                if (!tasks.Any()) continue;

                await Write(tasks, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // we're on out way out
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Unhandled exception in FasterLog writer task");
            }
        }

        _logger.Debug("Stopped FasterLog serialized writer task");
    }

    async Task Write(IEnumerable<WriteTask> tasks, CancellationToken cancellationToken)
    {
        var topicGroups = tasks.GroupBy(t => t.Topic);

        async Task WriteToTopic(IGrouping<string, WriteTask> group)
        {
            await Write(topic: group.Key, tasks: group.ToList(), cancellationToken: cancellationToken);
        }

        await Task.WhenAll(topicGroups.Select(WriteToTopic));
    }

    async Task Write(string topic, IReadOnlyList<WriteTask> tasks, CancellationToken cancellationToken)
    {
        try
        {
            var log = _deviceManager.GetLog(topic);

            for (var index = 0; index < tasks.Count; index++)
            {
                foreach (var transportMessage in tasks[index].TransportMessages)
                {
                    var bytes = _logEntrySerializer.Serialize(transportMessage);

                    await log.EnqueueAsync(bytes, cancellationToken);
                }
            }

            await log.CommitAsync(cancellationToken);

            for (var index = 0; index < tasks.Count; index++)
            {
                tasks[index].Succeed();
            }

            _eventExpirationHelper.RegisterActivity(topic);
        }
        catch (Exception exception)
        {
            for (var index = 0; index < tasks.Count; index++)
            {
                tasks[index].Fail(exception);
            }
        }
    }

    public void Dispose()
    {
        try
        {
            // not initialized or already disposed? who cares
            if (_writer == null) return;

            _logger.Info("Disposing FasterLog producer");
            _cancellationTokenSource.Cancel();

            var timeout = TimeSpan.FromSeconds(4);

            if (!_writer.WaitSafe(timeout))
            {
                _logger.Warn("Background writer task did not exit within timeout of {timeout}", timeout);
            }
        }
        finally
        {
            _writer = null;
            _cancellationTokenSource.Dispose();
        }
    }

    IReadOnlyList<WriteTask> DequeueNext(int maxCount)
    {
        var list = new List<WriteTask>(capacity: Math.Min(_writeTasks.Count, maxCount));

        while (_writeTasks.TryDequeue(out var task))
        {
            list.Add(task);
        }

        return list;
    }

    class WriteTask
    {
        readonly TaskCompletionSource<object> _taskCompletionSource = new();
        readonly CancellationTokenRegistration _cancellationRegistration;

        public string Topic { get; }
        public string PartitionKey { get; }
        public IEnumerable<TransportMessage> TransportMessages { get; }

        public WriteTask(string topic, string partitionKey, IEnumerable<TransportMessage> transportMessages, CancellationToken cancellationToken)
        {
            Topic = topic;
            PartitionKey = partitionKey;
            TransportMessages = transportMessages;
            _cancellationRegistration = cancellationToken.Register(() => _taskCompletionSource.TrySetCanceled(cancellationToken));
        }

        public Task Task => _taskCompletionSource.Task;

        public void Succeed() => Task.Run(() =>
        {
            _taskCompletionSource.SetResult(null);
            _cancellationRegistration.Dispose();
        });

        public void Fail(Exception exception) => Task.Run(() =>
        {
            _taskCompletionSource.SetException(exception);
            _cancellationRegistration.Dispose();
        });
    }
}