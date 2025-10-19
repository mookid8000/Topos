using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Topos.Extensions;
using Topos.Logging;
using Topos.Serialization;

namespace Topos.Producer;

public class DefaultToposProducer : IToposProducer
{
    readonly IMessageSerializer _messageSerializer;
    readonly IProducerImplementation _producerImplementation;
    readonly ILogger _logger;

    bool _disposing;
    bool _disposed;

    public event Action Disposing;

    public DefaultToposProducer(IMessageSerializer messageSerializer, IProducerImplementation producerImplementation, ILoggerFactory loggerFactory)
    {
        if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
        _logger = loggerFactory.GetLogger(typeof(DefaultToposProducer));
        _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
        _producerImplementation = producerImplementation ?? throw new ArgumentNullException(nameof(producerImplementation));
    }

    public async Task SendAsync(string topic, ToposMessage message, string partitionKey = null, CancellationToken cancellationToken = default)
    {
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        if (message == null) throw new ArgumentNullException(nameof(message));

        var transportMessage = GetTransportMessage(message);

        _logger.Debug("Sending message with ID {messageId} to topic {topic}", transportMessage.GetMessageId(), topic);

        await _producerImplementation.SendAsync(topic, partitionKey, transportMessage, cancellationToken);
    }

    public async Task SendManyAsync(string topic, IEnumerable<ToposMessage> messages, string partitionKey = null, CancellationToken cancellationToken = default)
    {
        if (topic == null) throw new ArgumentNullException(nameof(topic));
        if (messages == null) throw new ArgumentNullException(nameof(messages));

        var transportMessages = messages.Select(GetTransportMessage);

        await _producerImplementation.SendManyAsync(topic, partitionKey, transportMessages, cancellationToken);
    }

    TransportMessage GetTransportMessage(ToposMessage message)
    {
        var body = message.Body;
        var headersOrNull = message.Headers;
        var headers = headersOrNull?.Clone() ?? new Dictionary<string, string>();

        if (!headers.ContainsKey(ToposHeaders.MessageId))
        {
            headers[ToposHeaders.MessageId] = Guid.NewGuid().ToString();
        }

        if (!headers.ContainsKey(ToposHeaders.Time))
        {
            headers[ToposHeaders.Time] = DateTimeOffset.Now.ToIso8601DateTimeOffset();
        }

        var logicalMessage = new LogicalMessage(headers, body);
        var transportMessage = _messageSerializer.Serialize(logicalMessage);
        return transportMessage;
    }

    /// <summary>
    /// Guard agains double-entry, because the callback will end up disposing this instance too
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        if (_disposing) return;

        _disposing = true;

        try
        {
            Disposing?.Invoke();
        }
        finally
        {
            _disposed = true;
        }
    }
}