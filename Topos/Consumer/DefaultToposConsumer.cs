using System;
using Topos.Serialization;

namespace Topos.Consumer
{
    public class DefaultToposConsumer : IToposConsumer
    {
        readonly IConsumerImplementation _consumerImplementation;

        bool _isStarted;

        bool _disposing;
        bool _disposed;

        public event Action Disposing;

        public DefaultToposConsumer(IConsumerImplementation consumerImplementation)
        {
            _consumerImplementation = consumerImplementation;
        }

        public IDisposable Start()
        {
            if (_isStarted) return this;

            _consumerImplementation.Start();
            _isStarted = true;

            return this;
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
}