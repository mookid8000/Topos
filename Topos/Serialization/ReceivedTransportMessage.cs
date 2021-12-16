using System.Collections.Generic;
using Topos.Consumer;

namespace Topos.Serialization;

public class ReceivedTransportMessage : TransportMessage
{
    public Position Position { get; }

    public ReceivedTransportMessage(Position position, Dictionary<string, string> headers, byte[] body) : base(headers, body)
    {
        Position = position;
    }
}