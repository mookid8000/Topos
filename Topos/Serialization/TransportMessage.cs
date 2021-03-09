using System;
using System.Collections.Generic;

namespace Topos.Serialization
{
    public class TransportMessage
    {
        public Dictionary<string, string> Headers { get; }
        public byte[] Body { get; }

        public TransportMessage(Dictionary<string, string> headers, byte[] body)
        {
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Body = body;
        }
    }
}
