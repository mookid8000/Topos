using System;
// ReSharper disable ArgumentsStyleNamedExpression

namespace Topos.Serialization;

public class RawMessageSerializer : IMessageSerializer
{
    public TransportMessage Serialize(LogicalMessage message)
    {
        if (!(message.Body is byte[] bytes))
        {
            throw new ArgumentException($"Cannot send message, because the message body {message.Body} is not a byte[]");
        }

        return new TransportMessage(headers: message.Headers, body: bytes);
    }

    public ReceivedLogicalMessage Deserialize(ReceivedTransportMessage message)
    {
        return new ReceivedLogicalMessage(
            headers: message.Headers,
            body: message.Body,
            position: message.Position
        );
    }
}