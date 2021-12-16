using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using Topos.Faster;
using Topos.Serialization;

namespace Topos.Internals;

class ProtobufLogEntrySerializer : ILogEntrySerializer
{
    public byte[] Serialize(TransportMessage transportMessage)
    {
        var entry = new FasterLogEntry
        {
            Headers = transportMessage.Headers,
            Body = transportMessage.Body
        };

        return Serialize(entry);
    }

    TransportMessage ILogEntrySerializer.Deserialize(byte[] bytes)
    {
        var entry = Deserialize(bytes);

        return new TransportMessage(entry.Headers, entry.Body);
    }

    static byte[] Serialize(FasterLogEntry entry)
    {
        using var destination = new MemoryStream();
        Serializer.Serialize(destination, entry);
        return destination.ToArray();
    }

    static FasterLogEntry Deserialize(byte[] bytes)
    {
        using var source = new MemoryStream(bytes);
        return Serializer.Deserialize<FasterLogEntry>(source);
    }

    [ProtoContract]
    struct FasterLogEntry
    {
        [ProtoMember(1)]
        public Dictionary<string, string> Headers { get; set; }
        [ProtoMember(2)]
        public byte[] Body { get; set; }
    }
}