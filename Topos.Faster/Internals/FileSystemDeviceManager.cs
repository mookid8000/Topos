using System;
using System.IO;
using FASTER.core;
using Topos.Consumer;
using Topos.Faster;
using Topos.Helpers;
using Topos.Logging;
#pragma warning disable 1998

namespace Topos.Internals;

class FileSystemDeviceManager : IInitializable, IDisposable, IDeviceManager
{
    readonly Disposables _disposables = new();
    readonly string _directoryPath;
    readonly ILogger _logger;

    public FileSystemDeviceManager(ILoggerFactory loggerFactory, string directoryPath)
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

    public FasterLog GetWriter(string topic) => InitializeLog(_directoryPath, topic);
    
    public FasterLog GetReader(string topic) => InitializeLog(_directoryPath, topic);

    FasterLog InitializeLog(string directoryPath, string topic)
    {
        var deviceKey = $"Type=Device;Directory={directoryPath};Topic={topic}";
        var logKey = $"Type=Log;Directory={directoryPath};Topic={topic}";

        var pooledDevice = SingletonPool.GetInstance(deviceKey, () =>
        {
            _logger.Debug("Initializing singleton file system device with key {key}", deviceKey);

            var logDirectory = Path.Combine(directoryPath, topic);

            EnsureDirectoryExists(logDirectory);

            var filePath = Path.Combine(logDirectory, $"{topic}.log");

            return Devices.CreateLogDevice(filePath, recoverDevice: true);
        });

        _disposables.Add(pooledDevice);

        var device = pooledDevice.Instance;

        var pooledLog = SingletonPool.GetInstance(logKey, () =>
        {
            _logger.Debug("Initializing singleton log instance with key {key}", logKey);

            var settings = new FasterLogSettings
            {
                LogDevice = device,
                PageSizeBits = 23   //< page size is 2^23 = 8 MB
            };

            return new FasterLog(settings, logger: new MicrosoftLoggerAdapter(_logger));
        });

        _disposables.Add(pooledLog);

        _logger.Debug("Singleton pool contains the following objs: {@objs}", SingletonPool.ActiveObjects);

        return pooledLog.Instance;
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

    public void Dispose() => _disposables.Dispose();
}