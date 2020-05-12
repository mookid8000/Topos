using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Topos.Serialization;

namespace Topos.Consumer
{
    /// <summary>
    /// Delegate type used for message handlers
    /// </summary>
    public delegate Task MessageHandlerDelegate(IReadOnlyCollection<ReceivedLogicalMessage> messages, ConsumerContext context, CancellationToken cancellationToken);
}