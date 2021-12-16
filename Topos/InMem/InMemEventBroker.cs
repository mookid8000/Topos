using System.Collections.Concurrent;
using System.Threading;
using Topos.Serialization;

namespace Topos.InMem;

public class InMemEventBroker
{
    readonly ConcurrentDictionary<string, ConcurrentQueue<InMemTransportMessage>> _messages = new ConcurrentDictionary<string, ConcurrentQueue<InMemTransportMessage>>();

    long _offsetCounter;

    public void Send(string topic, TransportMessage message)
    {
        var inMemTransportMessage = new InMemTransportMessage(
            transportMessage: message,
            offset: Interlocked.Increment(ref _offsetCounter)
        );

        GetQueueForTopic(topic).Enqueue(inMemTransportMessage);
    }

    ConcurrentQueue<InMemTransportMessage> GetQueueForTopic(string topic) => _messages.GetOrAdd(topic, _ => new ConcurrentQueue<InMemTransportMessage>());

    class InMemTransportMessage
    {
        public TransportMessage TransportMessage { get; }
        public long Offset { get; }

        public InMemTransportMessage(TransportMessage transportMessage, long offset)
        {
            TransportMessage = transportMessage;
            Offset = offset;
        }
    }
}