using System.Collections.Generic;
using System.Linq;
using Topos.Consumer;
using Topos.Logging;

namespace Topos.FileSystem
{
    class FileSystemConsumerImplementation : IConsumerImplementation
    {
        readonly IConsumerDispatcher _consumerDispatcher;
        readonly IPositionManager _positionManager;
        readonly ILoggerFactory _loggerFactory;
        readonly string[] _topics;

        public FileSystemConsumerImplementation(string directoryPath, ILoggerFactory loggerFactory, IEnumerable<string> topics, string group,
            IConsumerDispatcher consumerDispatcher, IPositionManager positionManager)
        {
            _loggerFactory = loggerFactory;
            _consumerDispatcher = consumerDispatcher;
            _positionManager = positionManager;
            _topics = topics.ToArray();
        }

        public void Start()
        {
        }
    }
}