using System.Collections.Concurrent;
using Topos.Serialization;

namespace Topos.InMem
{
    public class InMemEventBroker
    {
        readonly ConcurrentDictionary<string, ConcurrentQueue<InMemTransportMessage>> _messages = new ConcurrentDictionary<string, ConcurrentQueue<InMemTransportMessage>>();

        public void Send(string topic, TransportMessage message)
        {
            var queue = GetQueueForTopic(topic);
            var inMemTransportMessage = new InMemTransportMessage(message, 1);

            queue.Enqueue(inMemTransportMessage);
        }

        ConcurrentQueue<InMemTransportMessage> GetQueueForTopic(string topic)
        {
            return _messages.GetOrAdd(topic, _ => new ConcurrentQueue<InMemTransportMessage>());
        }

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
}