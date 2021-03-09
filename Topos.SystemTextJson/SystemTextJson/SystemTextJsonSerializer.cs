using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Topos.Extensions;
using Topos.Serialization;

namespace Topos.SystemTextJson
{
    public class SystemTextJsonSerializer : IMessageSerializer
    {
        const string DefaultContentType = "application/json; charset=utf-8";

        public TransportMessage Serialize(LogicalMessage message)
        {
            var headers = message.Headers.Clone();
            var body = message.Body;

            headers[ToposHeaders.ContentType] = DefaultContentType;
            headers[ToposHeaders.MessageType] = body.GetType().GetSimpleAssemblyQualifiedTypeName();

            var bytes = JsonSerializer.SerializeToUtf8Bytes(body);

            return new TransportMessage(headers, bytes);
        }

        public ReceivedLogicalMessage Deserialize(ReceivedTransportMessage message)
        {
            var headers = message.Headers.Clone();

            GetValue(headers, ToposHeaders.ContentType, out var contentType);

            if (!string.Equals(contentType, DefaultContentType, StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException($"Cannot decode content type '{contentType}' yet, only '{DefaultContentType}' is understood");
            }

            GetValue(headers, ToposHeaders.MessageType, out var messageType);

            var bytes = message.Body;

            try
            {
                var type = messageType.ParseType();
                var body = JsonSerializer.Deserialize(bytes, type);
                var position = message.Position;
                return new ReceivedLogicalMessage(headers, body, position);
            }
            catch (Exception exception)
            {
                var json = Encoding.UTF8.GetString(bytes);

                throw new FormatException($"Could not deserialize JSON text: '{json}'", exception);
            }
        }

        static void GetValue(Dictionary<string, string> headers, string key, out string value)
        {
            var foundHeader = headers.TryGetValue(key, out value);

            if (!foundHeader)
            {
                throw new FormatException($"Did not find '{key}' header on the message");
            }
        }
    }
}
