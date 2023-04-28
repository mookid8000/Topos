using System;
using System.Threading;
using FASTER.core;
using FASTER.devices;
using Topos.Consumer;
using Topos.Faster;
using Topos.Helpers;
using Topos.Logging;
#pragma warning disable 1998

namespace Topos.Internals;

class BlobStorageDeviceManager : IInitializable, IDisposable, IDeviceManager
{
    readonly Disposables _disposables = new();
    readonly string _connectionString;
    readonly string _containerName;
    readonly ILogger _logger;

    public BlobStorageDeviceManager(ILoggerFactory loggerFactory, string connectionString, string containerName)
    {
        if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _containerName = containerName ?? throw new ArgumentNullException(nameof(containerName));

        if (!AzureBlobsHelper.IsValidConnectionString(_connectionString))
        {
            throw new ArgumentException($"The connection string '{connectionString}' does not look like a valid Azure storage connection string");
        }

        _logger = loggerFactory.GetLogger(GetType());
    }

    public void Initialize()
    {
        _logger.Info("Initializing Azure Blobs device manager for container {containerName}", _containerName);
    }

    public FasterLog GetWriter(string rawTopic, CancellationToken cancellationToken)
    {
        var topic = SanitizeTopicName(rawTopic);

        var deviceKey = $"Type=Device;ConnectionString={_connectionString};ContainerName={_containerName};Topic={topic}";
        var managerKey = $"Type=Manager;ConnectionString={_connectionString};ContainerName={_containerName};Topic={topic}";
        var logKey = $"Type=Log;ConnectionString={_connectionString};ContainerName={_containerName};Topic={topic}";

        var loggerAdapter = new MicrosoftLoggerAdapter(_logger);

        var pooledDevice = SingletonPool.GetInstance(deviceKey, () =>
        {
            _logger.Debug("Initializing singleton Azure Blobs device with key {key}", deviceKey);

            if (new AzureBlobsHelper(_connectionString).CreateContainerIfNotExists(_containerName))
            {
                _logger.Info("Successfully created blob container {containerName}", _containerName);
            }

            return new AzureStorageDevice(
                connectionString: _connectionString,
                containerName: _containerName,
                directoryName: topic,
                blobName: "data",
                logger: loggerAdapter,
                underLease: true
            );
        });

        _disposables.Add(pooledDevice);

        var device = pooledDevice.Instance;

        var pooledCheckpointManager = SingletonPool.GetInstance(managerKey, () =>
        {
            _logger.Debug("Initializing singleton Azure Blobs checkpoint manager with key {key}", managerKey);

            var deviceFactory = new AzureStorageNamedDeviceFactory(_connectionString, logger: loggerAdapter);
            var namingScheme = new DefaultCheckpointNamingScheme(baseName: $"{_containerName}/{topic}");

            return new DeviceLogCommitCheckpointManager(deviceFactory, namingScheme, logger: loggerAdapter);
        });

        _disposables.Add(pooledCheckpointManager);

        var checkpointManager = pooledCheckpointManager.Instance;

        var pooledLog = SingletonPool.GetInstance(logKey, () =>
        {
            _logger.Debug("Initializing singleton log instance with key {key}", logKey);

            var settings = new FasterLogSettings
            {
                ReadOnlyMode = false,
                LogCommitManager = checkpointManager,
                LogDevice = device,
                PageSizeBits = 23   //< page size is 2^23 = 8 MB
            };

            return new FasterLog(settings, logger: loggerAdapter);
        });

        _disposables.Add(pooledLog);

        _logger.Debug("Singleton pool contains the following objs: {@objs}", SingletonPool.ActiveObjects);

        return pooledLog.Instance;
    }

    public FasterLog GetReader(string rawTopic, CancellationToken cancellationToken)
    {
        var writer = GetWriter(rawTopic, cancellationToken);
        writer.Enqueue(FasterLogConsumerImplementation.DummyData);
        writer.Commit(spinWait: true);

        var topic = SanitizeTopicName(rawTopic);

        var deviceKey = $"Type=Device;ConnectionString={_connectionString};ContainerName={_containerName};Topic={topic}";
        var logKey = $"Type=Log;ConnectionString={_connectionString};ContainerName={_containerName};Topic={topic};Readonly={true}";

        var loggerAdapter = new MicrosoftLoggerAdapter(_logger);

        var pooledDevice = SingletonPool.GetInstance(deviceKey, () =>
        {
            _logger.Debug("Initializing singleton Azure Blobs device with key {key}", deviceKey);

            if (new AzureBlobsHelper(_connectionString).CreateContainerIfNotExists(_containerName))
            {
                _logger.Info("Successfully created blob container {containerName}", _containerName);
            }

            return new AzureStorageDevice(
                connectionString: _connectionString,
                containerName: _containerName,
                directoryName: topic,
                blobName: "data",
                logger: loggerAdapter,
                underLease: true
            );
        });

        _disposables.Add(pooledDevice);

        var device = pooledDevice.Instance;

        var pooledLog = SingletonPool.GetInstance(logKey, () =>
        {
            _logger.Debug("Initializing singleton log instance with key {key}", logKey);

            var settings = new FasterLogSettings
            {
                ReadOnlyMode = true,
                LogDevice = device,
                PageSizeBits = 23   //< page size is 2^23 = 8 MB
            };

            return new FasterLog(settings, logger: loggerAdapter);
        });

        _disposables.Add(pooledLog);

        _logger.Debug("Singleton pool contains the following objs: {@objs}", SingletonPool.ActiveObjects);

        return pooledLog.Instance;
    }

    static string SanitizeTopicName(string topic) => topic;

    public void Dispose() => _disposables.Dispose();
}
