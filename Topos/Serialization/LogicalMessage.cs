using System;
using System.Collections.Generic;

namespace Topos.Serialization;

public class LogicalMessage(Dictionary<string, string> headers, object body)
{
    public Dictionary<string, string> Headers { get; } = headers ?? throw new ArgumentNullException(nameof(headers));
    public object Body { get; } = body ?? throw new ArgumentNullException(nameof(body));
}