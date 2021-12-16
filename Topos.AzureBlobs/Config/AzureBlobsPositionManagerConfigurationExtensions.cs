using System;
using Microsoft.Azure.Storage;
using Topos.AzureBlobs;
using Topos.Consumer;
// ReSharper disable UnusedMember.Global

namespace Topos.Config;

public static class AzureBlobsPositionManagerConfigurationExtensions
{
    /// <summary>
    /// Configures Topos to stores its consumer positions in blobs. Individual blobs will be created for each relevant topic/partition.
    /// </summary>
    public static void StoreInAzureBlobs(this StandardConfigurer<IPositionManager> configurer, CloudStorageAccount storageAccount, string containerName)
    {
        if (configurer == null) throw new ArgumentNullException(nameof(configurer));
        if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));
        if (containerName == null) throw new ArgumentNullException(nameof(containerName));

        var registrar = StandardConfigurer.Open(configurer);

        registrar.Register(_ => new AzureBlobsPositionManager(storageAccount, containerName));
    }
}