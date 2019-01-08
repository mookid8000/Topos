using System;

namespace Topos.Internals.Consumer
{
    class ToposConsumer : IToposConsumer
    {
        bool _disposed;

        public IDisposable Start()
        {
            return this;
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {

            }
            finally
            {
                _disposed = true;
            }
        }
    }
}