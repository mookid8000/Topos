using System;
using System.Collections.Concurrent;
using System.IO;
using FASTER.core;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Topos.Consumer;
using Topos.Faster;
using Topos.Helpers;
using Topos.Logging;
#pragma warning disable 1998

namespace Topos.Internals;

class BlobStorageDeviceManager : IInitializable, IDisposable, IDeviceManager
{
    readonly CloudStorageAccount _cloudStorageAccount;
    readonly ConcurrentDictionary<string, Lazy<FasterLog>> _logs = new();
    readonly Disposables _disposables = new();
    readonly ILogger _logger;

    public BlobStorageDeviceManager(ILoggerFactory loggerFactory, CloudStorageAccount cloudStorageAccount)
    {
        if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
        _cloudStorageAccount = cloudStorageAccount ?? throw new ArgumentNullException(nameof(cloudStorageAccount));
        _logger = loggerFactory.GetLogger(GetType());
    }

    public void Initialize()
    {
        //_logger.Info("Initializing device manager with directory {directoryPath}", _directoryPath);

        //EnsureDirectoryExists(_directoryPath);
    }

    public FasterLog GetLog(string topic) => _logs.GetOrAdd(topic, _ => new Lazy<FasterLog>(() => InitializeLog(topic))).Value;

    FasterLog InitializeLog(string topic)
    {
        //new FASTER.devices.AzureStorageDevice(_cloudStorageAccount.CreateCloudBlobClient().)
        var directoryPath = "";
        var deviceKey = $"Type=Device;Directory={directoryPath};Topic={topic}";
        var logKey = $"Type=Log;Directory={directoryPath};Topic={topic}";

        var pooledDevice = SingletonPool.GetInstance(deviceKey, () =>
        {
            var logDirectory = Path.Combine(directoryPath, topic);

            EnsureDirectoryExists(logDirectory);

            var filePath = Path.Combine(logDirectory, $"{topic}.log");
            return Devices.CreateLogDevice(filePath);
        });

        _disposables.Add(pooledDevice);

        var device = pooledDevice.Instance;

        var pooledLog = SingletonPool.GetInstance(logKey, () =>
        {
            var log = new FasterLog(new FasterLogSettings
            {
                LogDevice = device,
                PageSizeBits = 23   //< page size is 2^23 = 8 MB
            });

            return log;
        });

        _disposables.Add(pooledLog);

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