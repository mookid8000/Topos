using System;
// ReSharper disable ArgumentsStyleNamedExpression

namespace Topos.Serialization;

public class RawMessageSerializer : IMessageSerializer
{
    public TransportMessage Serialize(LogicalMessage message) =>
        message.Body is byte[] bytes
            ? new TransportMessage(headers: message.Headers, body: bytes)
            : throw new ArgumentException($"Cannot send message, because the message body {message.Body} is not a byte[]");

    public ReceivedLogicalMessage Deserialize(ReceivedTransportMessage message) =>
        new(
            headers: message.Headers,
            body: message.Body,
            position: message.Position
        );
}