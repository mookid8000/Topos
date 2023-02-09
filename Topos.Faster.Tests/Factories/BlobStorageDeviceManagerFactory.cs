using System;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Topos.Internals;
using Topos.Logging.Console;
using LogLevel = Topos.Logging.Console.LogLevel;

namespace Topos.Faster.Tests.Factories;

public class BlobStorageDeviceManagerFactory : IDeviceManagerFactory
{
    readonly CloudStorageAccount _storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
    readonly string _containerName = Guid.NewGuid().ToString("N");

    public IDeviceManager Create() => new BlobStorageDeviceManager(new ConsoleLoggerFactory(LogLevel.Debug), _storageAccount);

    public void Dispose() => _storageAccount.CreateCloudBlobClient().GetContainerReference(_containerName).DeleteIfExists();
}