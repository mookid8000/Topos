using System;

namespace Topos.Consumer;

public class DefaultToposConsumer(IConsumerImplementation consumerImplementation) : IToposConsumer
{
    bool _isStarted;

    bool _disposing;
    bool _disposed;

    public event Action Disposing;

    public void Start()
    {
        if (_isStarted) return;

        consumerImplementation.Start();
        
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