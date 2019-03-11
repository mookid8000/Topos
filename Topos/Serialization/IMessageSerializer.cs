namespace Topos.Serialization
{
    public interface IMessageSerializer
    {
        TransportMessage Serialize(LogicalMessage message);
        LogicalMessage Deserialize(TransportMessage message);
    }
}