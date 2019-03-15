using System;
using System.Threading.Tasks;
using Topos.Serialization;

namespace Topos
{
    /// <summary>
    /// Implement this to create a producer for a concrete transport.
    /// </summary>
    public interface IProducerImplementation : IDisposable
    {
        /// <summary>
        /// Must send the specified message to the specified topic, ensuring ordered delivery within the specified partition key.
        /// </summary>
        /// <param name="topic">Name of the topic to send the message to</param>
        /// <param name="partitionKey">Key which will be used to pick a partition to append the message to</param>
        /// <param name="transportMessage">Raw transport message in the form of bytes and headers</param>
        /// <returns></returns>
        Task Send(string topic, string partitionKey, TransportMessage transportMessage);
    }
}