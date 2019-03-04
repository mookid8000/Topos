using System;
using Topos.Logging;

namespace Topos.Internals.Consumer
{
    class ToposConsumer : IToposConsumer
    {
        readonly ILogger _logger;

        bool _disposed;

        public ToposConsumer(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLogger(typeof(ToposConsumer));
        }

        public IDisposable Start()
        {
            _logger.Info("Starting consumer");

            return this;
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _logger.Info("Disposing consumer");

            }
            finally
            {
                _disposed = true;
            }
        }
    }
}