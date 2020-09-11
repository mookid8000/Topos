using Topos.Serialization;

namespace Topos.Faster
{
    public interface ILogEntrySerializer
    {
        byte[] Serialize(string partitionKey, TransportMessage transportMessage);
    }
}