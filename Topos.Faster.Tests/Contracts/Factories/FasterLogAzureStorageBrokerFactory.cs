using System;
using Topos.Config;
using Topos.Faster.Tests.Factories;
using Topos.Tests.Contracts;
using Topos.Tests.Contracts.Factories;

namespace Topos.Faster.Tests.Contracts.Factories;

public class FasterLogAzureStorageBrokerFactory : DisposableFactory, IBrokerFactory
{
    readonly string _containerName = Guid.NewGuid().ToString("n");

    int _counter;

    public FasterLogAzureStorageBrokerFactory() => Using(new StorageContainerDeleter(_containerName));

    public ToposProducerConfigurer ConfigureProducer() =>
        Configure
            .Producer(p => p.UseAzureStorage(BlobStorageDeviceManagerFactory.StorageConnectionString, _containerName))
            .Logging(l => l.UseSerilog());

    public ToposConsumerConfigurer ConfigureConsumer(string groupName) =>
        Configure
            .Consumer(groupName, c => c.UseAzureStorage(BlobStorageDeviceManagerFactory.StorageConnectionString, _containerName))
            .Logging(l => l.UseSerilog());

    public string GetNewTopic() => $"topic{_counter++}";
}