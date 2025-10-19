using System.Collections.Generic;
using Topos.Consumer;

namespace Topos.Serialization;

public class ReceivedTransportMessage(Position position, Dictionary<string, string> headers, byte[] body)
    : TransportMessage(headers, body)
{
    public Position Position { get; } = position;
}