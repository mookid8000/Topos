using System;
using System.Collections.Generic;

namespace Topos.Serialization;

public class LogicalMessage
{
    public Dictionary<string, string> Headers { get; }
    public object Body { get; }

    public LogicalMessage(Dictionary<string, string> headers, object body)
    {
        Headers = headers ?? throw new ArgumentNullException(nameof(headers));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }
}