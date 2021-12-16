using System;

namespace Topos.Consumer;

public class DefaultToposConsumer : IToposConsumer
{
    readonly IConsumerImplementation _consumerImplementation;

    bool _isStarted;

    bool _disposing;
    bool _disposed;

    public event Action Disposing;

    public DefaultToposConsumer(IConsumerImplementation consumerImplementation)
    {
        _consumerImplementation = consumerImplementation ?? throw new ArgumentNullException(nameof(consumerImplementation));
    }

    public void Start()
    {
        if (_isStarted) return;

        _consumerImplementation.Start();
        _isStarted = true;
    }

    public void Dispose()
    {
        if (_disposed) return;
        if (_disposing) return;

        _disposing = true;

        try
        {
            Disposing?.Invoke();
        }
        finally
        {
            _disposed = true;
        }
    }
}