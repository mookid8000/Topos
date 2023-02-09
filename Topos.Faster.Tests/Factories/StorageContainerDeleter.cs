using System;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace Topos.Faster.Tests.Factories;

class StorageContainerDeleter : IDisposable
{
    private readonly string _containerName;

    public StorageContainerDeleter(string containerName) => _containerName = containerName ?? throw new ArgumentNullException(nameof(containerName));

    public void Dispose() =>
        CloudStorageAccount.Parse(BlobStorageDeviceManagerFactory.StorageConnectionString)
            .CreateCloudBlobClient().GetContainerReference(_containerName).DeleteIfExists();
}