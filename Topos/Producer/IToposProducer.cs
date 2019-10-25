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
        /// The message to send. The message is always a <see cref="ToposMessage"/>.
        /// Its <see cref="ToposMessage.Body"/> will be serialized using the configured serializer, and the topic will be
        /// determined by the current topic mapper. An optional dictionary of <see cref="ToposMessage.Headers"/> can be added, too.
        /// </param>
        /// <param name="partitionKey">
        /// Optional parameter that will be used to determine which partition the sent message will be sent to.
        /// Remember that if the order of the messages is important, you must specify a partition key that ensures partition affinity.
        /// </param>
        Task Send(ToposMessage message, string partitionKey = null);

        ///// <summary>
        ///// Sends the given messages.
        ///// </summary>
        ///// <param name="messages">
        ///// The messages to send. The messages are always a sequence of <see cref="ToposMessage"/>.
        ///// Each <see cref="ToposMessage.Body"/> will be serialized using the configured serializer, and the topic will be
        ///// determined by the current topic mapper. An optional dictionary of <see cref="ToposMessage.Headers"/> can be added, too.
        ///// </param>
        ///// <param name="partitionKey">
        ///// Optional parameter that will be used to determine which partition the sent messages will be sent to.
        ///// Remember that if the order of the messages is important, you must specify a partition key that ensures partition affinity.
        ///// </param>
        //Task SendMany(IEnumerable<ToposMessage> messages, string partitionKey = null);
    }
}