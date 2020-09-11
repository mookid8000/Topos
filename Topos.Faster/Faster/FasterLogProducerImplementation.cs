using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FASTER.core;
using Topos.Consumer;
using Topos.Logging;
using Topos.Serialization;
// ReSharper disable ForCanBeConvertedToForeach
#pragma warning disable 1998

namespace Topos.Faster
{
    class FasterLogProducerImplementation : IProducerImplementation, IInitializable
    {
        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        readonly ConcurrentQueue<WriteTask> _writeTasks = new ConcurrentQueue<WriteTask>();
        readonly IDeviceManager _deviceManager;
        readonly ILogEntrySerializer _logEntrySerializer;
        readonly ILogger _logger;

        Task _writer;

        public FasterLogProducerImplementation(ILoggerFactory loggerFactory, IDeviceManager deviceManager, ILogEntrySerializer logEntrySerializer)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _deviceManager = deviceManager ?? throw new ArgumentNullException(nameof(deviceManager));
            _logEntrySerializer = logEntrySerializer ?? throw new ArgumentNullException(nameof(logEntrySerializer));
            _logger = loggerFactory.GetLogger(GetType());
        }

        public Task Send(string topic, string partitionKey, TransportMessage transportMessage)
        {
            var writeTask = new WriteTask(topic, partitionKey, new[] { transportMessage });
            _writeTasks.Enqueue(writeTask);
            return writeTask.Task;
        }

        public Task SendMany(string topic, string partitionKey, IEnumerable<TransportMessage> transportMessages)
        {
            var writeTask = new WriteTask(topic, partitionKey, transportMessages);
            _writeTasks.Enqueue(writeTask);
            return writeTask.Task;
        }

        public void Initialize()
        {
            if (_writer != null) throw new InvalidOperationException("Attempted to initialize FasterLogProducerImplementation, but it has been initialized already!");

            _logger.Info("Initializing FasterLog producer");

            _writer = Task.Run(WriterTask);
        }

        async Task WriterTask()
        {
            var cancellationToken = _cancellationTokenSource.Token;

            _logger.Debug("Starting FasterLog serialized writer task");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var tasks = DequeueNext(100);

                    if (!tasks.Any())
                    {
                        await Task.Delay(TimeSpan.FromSeconds(0.2), cancellationToken);
                        continue;
                    }

                    await Write(tasks);
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

        async Task Write(IEnumerable<WriteTask> tasks)
        {
            await Task.WhenAll(
                tasks
                    .GroupBy(t => t.Topic)
                    .Select(async group => await Task.Run(async () => Write(
                        topic: group.Key,
                        tasks: group.ToList()
                    )))
            );
        }

        async Task Write(string topic, IReadOnlyList<WriteTask> tasks)
        {
            try
            {
                var log = _deviceManager.GetLog(topic);

                foreach (var task in tasks)
                {
                    Write(log, task);
                }

                await log.CommitAsync();

                for (var index = 0; index < tasks.Count; index++)
                {
                    tasks[index].Succeed();
                }
            }
            catch (Exception exception)
            {
                for (var index = 0; index < tasks.Count; index++)
                {
                    tasks[index].Fail(exception);
                }
            }
        }

        void Write(FasterLog log, WriteTask task)
        {
            foreach (var transportMessage in task.TransportMessages)
            {
                var bytes = _logEntrySerializer.Serialize(transportMessage);

                log.Enqueue(bytes);
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

                if (!_writer.Wait(timeout))
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
            var list = new List<WriteTask>(maxCount);

            while (_writeTasks.TryDequeue(out var task))
            {
                list.Add(task);
            }

            return list;
        }

        class WriteTask
        {
            readonly TaskCompletionSource<object> _taskCompletionSource = new TaskCompletionSource<object>();

            public string Topic { get; }
            public string PartitionKey { get; }
            public IEnumerable<TransportMessage> TransportMessages { get; }

            public WriteTask(string topic, string partitionKey, IEnumerable<TransportMessage> transportMessages)
            {
                Topic = topic;
                PartitionKey = partitionKey;
                TransportMessages = transportMessages;
            }

            public Task Task => _taskCompletionSource.Task;

            public void Succeed() => _taskCompletionSource.SetResult(null);

            public void Fail(Exception exception) => _taskCompletionSource.SetException(exception);
       }
    }
}