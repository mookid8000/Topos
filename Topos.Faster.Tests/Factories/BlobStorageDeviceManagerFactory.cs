using System;
using Topos.Internals;
using Topos.Logging.Console;
using LogLevel = Topos.Logging.Console.LogLevel;

namespace Topos.Faster.Tests.Factories;

public class BlobStorageDeviceManagerFactory : IDeviceManagerFactory
{
    public const string StorageConnectionString = "UseDevelopmentStorage=true;";

    readonly string _containerName = Guid.NewGuid().ToString("N");

    public IDeviceManager Create() => new BlobStorageDeviceManager(new ConsoleLoggerFactory(LogLevel.Debug), StorageConnectionString, _containerName, "db");

    public void Dispose() => new StorageContainerDeleter(_containerName).Dispose();
}