using Topos.Serialization;

namespace Topos.Consumer
{
    public interface IConsumerDispatcher
    {
        void Dispatch(ReceivedTransportMessage transportMessage);
    }
}