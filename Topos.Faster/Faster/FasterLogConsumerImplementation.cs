using System.Collections.Generic;
using Topos.Consumer;
using Topos.Logging;

namespace Topos.Faster
{
    class FasterLogConsumerImplementation : IConsumerImplementation
    {
        public FasterLogConsumerImplementation(string directoryPath, ILoggerFactory loggerFactory, IEnumerable<string> topics, string group, IConsumerDispatcher consumerDispatcher, IPositionManager positionManager)
        {

        }

        public void Start()
        {

        }
    }
}