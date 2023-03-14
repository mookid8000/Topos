using System;
using Topos.Internals;

namespace Topos.Faster.Tests.Factories;

class StorageContainerDeleter : IDisposable
{
    readonly string _containerName;

    public StorageContainerDeleter(string containerName) => _containerName = containerName ?? throw new ArgumentNullException(nameof(containerName));

    public void Dispose() =>
        new AzureBlobsHelper(BlobStorageDeviceManagerFactory.StorageConnectionString)
            .GetBlobContainerClient(_containerName).DeleteIfExists();
}