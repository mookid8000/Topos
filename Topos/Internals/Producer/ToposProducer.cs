using System.Collections.Generic;
using System.Threading.Tasks;
using Topos.Logging;

namespace Topos.Internals.Producer
{
    class ToposProducer : IToposProducer
    {
        readonly ILogger _logger;

        bool _disposed;

        public ToposProducer(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLogger(typeof(ToposProducer));
        }

        public async Task Send(object message, IDictionary<string, string> optionalHeaders = null)
        {
            
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _logger.Info("Disposing producer");

            }
            finally
            {
                _disposed = true;
            }
        }
    }
}