using System.Collections.Generic;
using Topos.Consumer;
using Topos.Logging;

namespace Topos.Faster
{
    class FasterLogFileSystemConsumerImplementation : IConsumerImplementation
    {
        public FasterLogFileSystemConsumerImplementation(string directoryPath, ILoggerFactory loggerFactory, IEnumerable<string> topics, string group, IConsumerDispatcher consumerDispatcher, IPositionManager positionManager)
        {

        }

        public void Start()
        {

        }
    }
}