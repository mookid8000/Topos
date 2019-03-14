using System;
using System.Threading.Tasks;
using Topos.Serialization;

namespace Topos
{
    public interface IToposProducerImplementation : IDisposable
    {
        Task Send(TransportMessage transportMessage);
    }
}