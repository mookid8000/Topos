using System;
using System.Linq;
using System.Threading;
using Topos.Config;
using Topos.Logging;
using Topos.Serialization;
// ReSharper disable ForCanBeConvertedToForeach

namespace Topos.Consumer
{
    public class DefaultConsumerDispatcher : IConsumerDispatcher, IInitializable, IDisposable
    {
        readonly ILoggerFactory _loggerFactory;
        readonly IMessageSerializer _messageSerializer;
        readonly MessageHandler[] _handlers;
        readonly ILogger _logger;

        public DefaultConsumerDispatcher(ILoggerFactory loggerFactory, IMessageSerializer messageSerializer, Handlers handlers)
        {
            if (handlers == null) throw new ArgumentNullException(nameof(handlers));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
            _handlers = handlers.ToArray();
            _logger = loggerFactory.GetLogger(typeof(DefaultConsumerDispatcher));
        }

        public void Initialize()
        {
            foreach(var handler in _handlers)
            {
                handler.Start(_loggerFactory.GetLogger(handler.GetType()));
            }
        }

        public void Dispatch(ReceivedTransportMessage transportMessage)
        {
            try
            {
                var logicalMessage = _messageSerializer.Deserialize(transportMessage);

                for (var index = 0; index < _handlers.Length; index++)
                {
                    var handler = _handlers[index];

                    while (!handler.IsReadyForMore)
                    {
                        Thread.Sleep(100);
                    }

                    handler.Enqueue(logicalMessage);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Error in dispatcher");
            }
        }

        public void Dispose()
        {
            foreach (var handler in _handlers)
            {
                handler.Stop();
            }

            foreach (var handler in _handlers)
            {
                handler.Dispose();
            }
        }
    }

}