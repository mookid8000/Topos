using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Topos.Producer
{
    /// <summary>
    /// This is the main entry point to a producer, which is what you use to send messages.
    /// </summary>
    public interface IToposProducer : IDisposable
    {
        /// <summary>
        /// Sends the given message.
        /// </summary>
        /// <param name="message">
        /// The message to send. It will be serialized using the configured serializer, and the topic will be
        /// determined by the current topic mapper.
        /// </param>
        /// <param name="partitionKey">
        /// Optional parameter that will be used to determine which partition the sent message will be sent to.
        /// Remember that if the order of the messages is important, you must specify a partition key that ensures partition affinity.
        /// </param>
        /// <param name="optionalHeaders">
        /// Optional parameter that specifies a collection of key-value pairs, which will be included along with the message.
        /// The headers may also be used by special features that may not be available across all transports.
        /// </param>
        /// <returns></returns>
        Task Send(object message, string partitionKey = null, Dictionary<string, string> optionalHeaders = null);
    }
}