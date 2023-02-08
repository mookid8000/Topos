//using System;
//using System.Collections.Concurrent;
//using System.IO;
//using FASTER.core;
//using Microsoft.Azure.Storage;
//using Topos.Consumer;
//using Topos.Faster;
//using Topos.Helpers;
//using Topos.Logging;
//#pragma warning disable 1998

//namespace Topos.Internals;

//class BlobStorageDeviceManager : IInitializable, IDisposable, IDeviceManager
//{
//    readonly ConcurrentDictionary<string, Lazy<FasterLog>> _logs = new();
//    readonly CloudStorageAccount _storageAccount;
//    readonly Disposables _disposables = new();
//    readonly ILogger _logger;

//    public BlobStorageDeviceManager(ILoggerFactory loggerFactory, string connectionString)
//    {
//        if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
//        if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

//        _storageAccount = CloudStorageAccount.TryParse(connectionString, out var result)
//            ? result
//            : throw new ArgumentException($"Could not parse the connection string '{connectionString}' as an Azure storage account connection string");

//        _logger = loggerFactory.GetLogger(GetType());
//    }

//    public void Initialize()
//    {
//        _logger.Info("Initializing device manager with directory {directoryPath}", _directoryPath);

//        EnsureDirectoryExists(_directoryPath);
//    }

//    public FasterLog GetLog(string topic) => _logs.GetOrAdd(topic, _ => new Lazy<FasterLog>(() => InitializeLog(_directoryPath, topic))).Value;

//    FasterLog InitializeLog(string directoryPath, string topic)
//    {
//        var deviceKey = $"Type=Device;Directory={directoryPath};Topic={topic}";
//        var logKey = $"Type=Log;Directory={directoryPath};Topic={topic}";

//        var pooledDevice = SingletonPool.GetInstance(deviceKey, () =>
//        {
//            var logDirectory = Path.Combine(directoryPath, topic);

//            EnsureDirectoryExists(logDirectory);

//            var filePath = Path.Combine(logDirectory, $"{topic}.log");
//            return Devices.CreateLogDevice(filePath);
//        });

//        _disposables.Add(pooledDevice);

//        var device = pooledDevice.Instance;

//        var pooledLog = SingletonPool.GetInstance(logKey, () =>
//        {
//            var log = new FasterLog(new FasterLogSettings
//            {
//                LogDevice = device,
//                PageSizeBits = 23   //< page size is 2^23 = 8 MB
//            });

//            return log;
//        });

//        _disposables.Add(pooledLog);

//        return pooledLog.Instance;
//    }

//    void EnsureDirectoryExists(string directoryPath)
//    {
//        if (Directory.Exists(directoryPath)) return;

//        try
//        {
//            _logger.Debug("Creating directory {directoryPath}", directoryPath);

//            Directory.CreateDirectory(directoryPath);
//        }
//        catch
//        {
//            if (!Directory.Exists(directoryPath))
//            {
//                throw;
//            }
//        }
//    }

//    public void Dispose() => _disposables.Dispose();
//}