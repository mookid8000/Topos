using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Storage.Blob;
using NUnit.Framework;
using Topos.Consumer;
using Topos.Tests.Contracts.Factories;
using Topos.Tests.Contracts.Positions;
// ReSharper disable CoVariantArrayConversion

namespace Topos.AzureBlobs.Tests
{
    [TestFixture]
    public class AzureBlobsPositionManagerTest : PositionsManagerTest<AzureBlobsPositionManagerTest.AzureBlobsPositionManagerFactory>
    {
        public class AzureBlobsPositionManagerFactory : IPositionsManagerFactory
        {
            readonly ConcurrentBag<string> _containersToRemove = new ConcurrentBag<string>();

            public IPositionManager Create()
            {
                var containerName = Guid.NewGuid().ToString("N");
                _containersToRemove.Add(containerName);
                return new AzureBlobsPositionManager(AzureBlobConfig.StorageAccount, containerName);
            }

            public void Dispose()
            {
                var client = AzureBlobConfig.StorageAccount.CreateCloudBlobClient();

                Task.WaitAll(
                    _containersToRemove
                        .Select(containerName => client.GetContainerReference(containerName).DeleteIfExistsAsync())
                        .ToArray()
                );
            }
        }
    }
}