using System.Collections.Generic;
using Topos.Consumer;

namespace Topos.Serialization
{
    public class ReceivedLogicalMessage : LogicalMessage
    {
        public Position Position { get; }

        public ReceivedLogicalMessage(Dictionary<string, string> headers, object body, Position position) : base(headers, body)
        {
            Position = position;
        }
    }
}