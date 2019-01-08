using System.Collections.Generic;
using System.Threading.Tasks;
using Topos.Logging;

namespace Topos.Internals.Producer
{
    class ToposProducer : IToposProducer
    {
        readonly ILogger _logger;

        public ToposProducer(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.GetLogger(typeof(ToposProducer));
        }

        public async Task Send(object message, IDictionary<string, string> optionalHeaders = null)
        {
            
        }
    }
}