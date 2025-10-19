using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Topos.Serialization;
#pragma warning disable 1998

namespace Topos.InMem;

class InMemProducerImplementation : IProducerImplementation
{
    readonly InMemEventBroker _eventBroker;

    public InMemProducerImplementation(InMemEventBroker eventBroker) => _eventBroker = eventBroker ?? throw new ArgumentNullException(nameof(eventBroker));

    public async Task SendAsync(string topic, string partitionKey, TransportMessage transportMessage, CancellationToken cancellationToken = default) => _eventBroker.Send(topic, transportMessage);

    public async Task SendManyAsync(string topic, string partitionKey, IEnumerable<TransportMessage> transportMessages, CancellationToken cancellationToken = default)
    {
        foreach (var transportMessage in transportMessages)
        {
            _eventBroker.Send(topic, transportMessage);
        }
    }

    public void Dispose()
    {
    }
}