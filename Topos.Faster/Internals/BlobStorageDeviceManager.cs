using System;
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
        _logger.Info("Initializing device manager with container {containerName} and directory {directoryName}",
            _containerName, _directoryName);
    }

    public FasterLog GetLog(string topic, bool @readonly = false) => _logs.GetOrAdd($"topic={topic};readonly={@readonly}", _ => new Lazy<FasterLog>(() => InitializeLog(topic, @readonly))).Value;

    FasterLog InitializeLog(string topic, bool @readonly)
    {
        var deviceKey = $"Type=Device;ConnectionString={_connectionString};ContainerName={_containerName};DirectoryName={_directoryName};Topic={topic};ReadOnly={@readonly}";
        var logKey = $"Type=Log;ConnectionString={_connectionString};ContainerName={_containerName};DirectoryName={_directoryName};Topic={topic};ReadOnly={@readonly}";

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
                directoryName: _directoryName,
                blobName: SanitizeTopicName(topic),
                logger: new MicrosoftLoggerAdapter(_logger)
            );
        });

        _disposables.Add(pooledDevice);

        var device = pooledDevice.Instance;

        var pooledLog = SingletonPool.GetInstance(logKey, () =>
        {
            _logger.Debug("Initializing singleton log instance with key {key}", logKey);

            var log = new FasterLog(new FasterLogSettings
            {
                ReadOnlyMode = @readonly,
                LogDevice = device,
                PageSizeBits = 23   //< page size is 2^23 = 8 MB
            });

            return log;
        });

        _disposables.Add(pooledLog);

        _logger.Debug("Singleton pool contains the following keys with refcount > 0: {keys}", SingletonPool.ActiveKeys);

        return pooledLog.Instance;
    }

    static string SanitizeTopicName(string topic) => topic;

    public void Dispose() => _disposables.Dispose();
}