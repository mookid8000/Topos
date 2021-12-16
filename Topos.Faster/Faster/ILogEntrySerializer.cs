using Topos.Serialization;

namespace Topos.Faster;

public interface ILogEntrySerializer
{
    byte[] Serialize(TransportMessage transportMessage);
    TransportMessage Deserialize(byte[] bytes);
}