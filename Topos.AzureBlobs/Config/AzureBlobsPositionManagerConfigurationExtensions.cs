using System;
using Microsoft.Azure.Storage;
using Topos.AzureBlobs;
using Topos.Consumer;

namespace Topos.Config
{
    public static class AzureBlobsPositionManagerConfigurationExtensions
    {
        public static void StoreInAzureBlobs(this StandardConfigurer<IPositionManager> configurer, CloudStorageAccount storageAccount, string containerName)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (storageAccount == null) throw new ArgumentNullException(nameof(storageAccount));
            if (containerName == null) throw new ArgumentNullException(nameof(containerName));

            var registrar = StandardConfigurer.Open(configurer);

            registrar.Register(c => new AzureBlobsPositionManager(storageAccount, containerName));
        }
    }
}