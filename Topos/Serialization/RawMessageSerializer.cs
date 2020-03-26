using System;

namespace Topos.Serialization
{
    public class RawMessageSerializer : IMessageSerializer
    {
        public TransportMessage Serialize(LogicalMessage message)
        {
            if (!(message.Body is byte[] bytes))
            {
                throw new ArgumentException($"Cannot send message, because the message body {message.Body} is not a byte[]");
            }

            return new TransportMessage(message.Headers, bytes);
        }

        public ReceivedLogicalMessage Deserialize(ReceivedTransportMessage message)
        {
            return new ReceivedLogicalMessage(message.Headers, message.Position, message.Position);
        }
    }
}