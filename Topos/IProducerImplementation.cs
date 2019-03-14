using System;
using System.Threading.Tasks;
using Topos.Serialization;

namespace Topos
{
    public interface IProducerImplementation : IDisposable
    {
        Task Send(string topic, string partitionKey, TransportMessage transportMessage);
    }
}