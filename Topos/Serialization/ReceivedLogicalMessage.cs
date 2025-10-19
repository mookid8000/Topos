using System.Collections.Generic;
using Topos.Consumer;

namespace Topos.Serialization;

public class ReceivedLogicalMessage(Dictionary<string, string> headers, object body, Position position)
    : LogicalMessage(headers, body)
{
    public Position Position { get; } = position;
}