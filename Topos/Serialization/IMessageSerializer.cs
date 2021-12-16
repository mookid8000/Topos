namespace Topos.Serialization;

public interface IMessageSerializer
{
    TransportMessage Serialize(LogicalMessage message);
    ReceivedLogicalMessage Deserialize(ReceivedTransportMessage message);
}