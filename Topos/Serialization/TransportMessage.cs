using System;
using System.Collections.Generic;

namespace Topos.Serialization;

public class TransportMessage(Dictionary<string, string> headers, byte[] body)
{
    public Dictionary<string, string> Headers { get; } = headers ?? throw new ArgumentNullException(nameof(headers));
    public byte[] Body { get; } = body;
}