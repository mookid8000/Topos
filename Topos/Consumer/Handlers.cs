using System.Collections;
using System.Collections.Generic;

namespace Topos.Consumer
{
    public class Handlers : IEnumerable<MessageHandler>
    {
        readonly List<MessageHandler> _messageHandlers = new List<MessageHandler>();
        
        public void Add(MessageHandler messageHandler) => _messageHandlers.Add(messageHandler);

        public IEnumerator<MessageHandler> GetEnumerator() => _messageHandlers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal IPositionManager PositionsManager { get; set; }
    }
}