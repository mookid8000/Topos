using System;
using Topos.Consumer;
using Topos.Serialization;

namespace Topos.Extensions
{
    public static class LogicalMessageExtensions
    {
        public static ReceivedLogicalMessage AsReceivedLogicalMessage(this LogicalMessage logicalMessage,
            Position position)
        {
            if (logicalMessage == null) throw new ArgumentNullException(nameof(logicalMessage));
            return new ReceivedLogicalMessage(logicalMessage.Headers, logicalMessage.Body, position);
        }
    }
}