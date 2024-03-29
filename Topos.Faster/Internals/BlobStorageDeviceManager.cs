﻿using System;
using System.Collections.Concurrent;
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
    readonly ConcurrentDictionary<string, Lazy<FasterLog>> _logs = new();
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

    public FasterLog GetLog(string topic) => _logs.GetOrAdd(topic, _ => new Lazy<FasterLog>(() => InitializeLog(topic))).Value;

    FasterLog InitializeLog(string topic)
    {
        var deviceKey = $"Type=Device;ConnectionString={_connectionString};ContainerName={_containerName};Topic={topic}";
        var managerKey = $"Type=Manager;ConnectionString={_connectionString};ContainerName={_containerName};Topic={topic}";
        var logKey = $"Type=Log;ConnectionString={_connectionString};ContainerName={_containerName};Topic={topic}";

        var loggerAdapter = new MicrosoftLoggerAdapter(_logger);
        var directoryName = SanitizeTopicName(topic);

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
                directoryName: directoryName,
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
            var namingScheme = new DefaultCheckpointNamingScheme(baseName: $"{_containerName}/{directoryName}");

            return new DeviceLogCommitCheckpointManager(deviceFactory, namingScheme, logger: loggerAdapter);
        });

        _disposables.Add(pooledCheckpointManager);

        var checkpointManager = pooledCheckpointManager.Instance;

        var pooledLog = SingletonPool.GetInstance(logKey, () =>
        {
            _logger.Debug("Initializing singleton log instance with key {key}", logKey);

            var settings = new FasterLogSettings
            {
                LogCommitManager = checkpointManager,
                LogDevice = device,
                PageSizeBits = 23   //< page size is 2^23 = 8 MB
            };

            return new FasterLog(settings, logger: loggerAdapter);
        });

        _disposables.Add(pooledLog);

        _logger.Debug("Singleton pool contains the following keys with refcount > 0: {keys}", SingletonPool.ActiveKeys);

        var log = pooledLog.Instance;

        if (log.CommittedUntilAddress == log.BeginAddress)
        {
            _logger.Debug("Detected un-initialized log for topic {topic} - will write dummy data", topic);
            log.Enqueue(FasterLogConsumerImplementation.DummyData);
            log.Commit(spinWait: true);
        }
        else
        {
            _logger.Debug("Detected log for topic {topic} was already initialized", topic);
        }

        return log;
    }

    static string SanitizeTopicName(string topic) => topic;

    public void Dispose() => _disposables.Dispose();
}
