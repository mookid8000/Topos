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
    readonly string _directoryName;
    readonly ILogger _logger;

    public BlobStorageDeviceManager(ILoggerFactory loggerFactory, string connectionString, string containerName, string directoryName)
    {
        if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _containerName = containerName ?? throw new ArgumentNullException(nameof(containerName));
        _directoryName = directoryName ?? throw new ArgumentNullException(nameof(directoryName));

        if (!AzureBlobsHelper.IsValidConnectionString(_connectionString))
        {
            throw new ArgumentException($"The connection string '{connectionString}' does not look like a valid Azure storage connection string");
        }

        _logger = loggerFactory.GetLogger(GetType());
    }

    public void Initialize()
    {
        _logger.Info("Initializing device manager with container {contaionerName} and directory {directoryName}",
            _containerName, _directoryName);
    }

    public FasterLog GetLog(string topic) => _logs.GetOrAdd(topic, _ => new Lazy<FasterLog>(() => InitializeLog(topic))).Value;

    FasterLog InitializeLog(string topic)
    {
        var deviceKey = $"Type=Device;ConnectionString={_connectionString};ContainerName={_containerName};DirectoryName={_directoryName};Topic={topic}";
        var logKey = $"Type=Log;ConnectionString={_connectionString};ContainerName={_containerName};DirectoryName={_directoryName};Topic={topic}";

        var pooledDevice = SingletonPool.GetInstance(deviceKey, () =>
        {
            if (new AzureBlobsHelper(_connectionString).CreateContainerIfNotExists(_containerName))
            {
                _logger.Info("Successfully created blob container {containerName}", _containerName);
            }

            return new AzureStorageDevice(
                connectionString: _connectionString,
                containerName: _containerName,
                directoryName: _directoryName,
                blobName: SanitizeTopicName(topic)
            );
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

    static string SanitizeTopicName(string topic) => topic;

    public void Dispose() => _disposables.Dispose();
}