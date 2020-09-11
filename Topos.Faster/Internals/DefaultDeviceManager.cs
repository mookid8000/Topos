using System;
using System.Collections.Concurrent;
using System.IO;
using FASTER.core;
using Topos.Consumer;
using Topos.Faster;
using Topos.Helpers;
using Topos.Logging;

namespace Topos.Internals
{
    class DefaultDeviceManager : IInitializable, IDisposable, IDeviceManager
    {
        readonly ConcurrentDictionary<string, Lazy<FasterLog>> _logs = new ConcurrentDictionary<string, Lazy<FasterLog>>();
        readonly Disposables _disposables = new Disposables();
        readonly string _directoryPath;
        readonly ILogger _logger;

        public DefaultDeviceManager(ILoggerFactory loggerFactory, string directoryPath)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _directoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
            _logger = loggerFactory.GetLogger(GetType());
        }

        public void Initialize()
        {
            _logger.Info("Initializing device manager with directory {directoryPath}", _directoryPath);

            EnsureDirectoryExists(_directoryPath);
        }

        public FasterLog GetLog(string topic) => _logs.GetOrAdd(topic, _ => new Lazy<FasterLog>(() => InitializeLog(_directoryPath, topic))).Value;

        FasterLog InitializeLog(string directoryPath, string topic)
        {
            var filePath = Path.Combine(directoryPath, $"{topic}.log");
            var device = Devices.CreateLogDevice(filePath);
            var log = new FasterLog(new FasterLogSettings { LogDevice = device });

            _disposables.Add(log);

            return log;
        }

        void EnsureDirectoryExists(string directoryPath)
        {
            if (Directory.Exists(directoryPath)) return;

            try
            {
                _logger.Debug("Creating directory {directoryPath}", directoryPath);
                
                Directory.CreateDirectory(directoryPath);
            }
            catch
            {
                if (!Directory.Exists(directoryPath))
                {
                    throw;
                }
            }
        }

        public void Dispose() => _disposables?.Dispose();
    }
}