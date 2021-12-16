using System;

namespace Topos.Consumer;

/// <summary>
/// This is a message consumer.
/// </summary>
public interface IToposConsumer : IDisposable
{
    /// <summary>
    /// Starts the consumer.
    /// </summary>
    /// <returns>A disposable that will stop message consumption when disposed.</returns>
    void Start();
}