using Topos.Serialization;

namespace Topos.Extensions;

public static class MessageExtensions
{
    public static string GetMessageId(this LogicalMessage message)
    {
        return message.Headers.GetValue(ToposHeaders.MessageId);
    }

    public static string GetMessageId(this TransportMessage message)
    {
        return message.Headers.GetValue(ToposHeaders.MessageId);
    }
}