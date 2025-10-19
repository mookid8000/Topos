using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Topos.Serialization;

namespace Topos;

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
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendAsync(string topic, string partitionKey, TransportMessage transportMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Must send the specified messages to the specified topic, ensuring ordered delivery within the specified partition key.
    /// </summary>
    /// <param name="topic">Name of the topic to send the message to</param>
    /// <param name="partitionKey">Key which will be used to pick a partition to append the message to</param>
    /// <param name="transportMessages">Sequence of raw transport messages in the form of bytes and headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SendManyAsync(string topic, string partitionKey, IEnumerable<TransportMessage> transportMessages, CancellationToken cancellationToken = default);
}