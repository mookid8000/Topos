using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Topos.Extensions;
using Topos.Serialization;

namespace Topos.NewtonsoftJson;

public class JsonSerializer : IMessageSerializer
{
    const string DefaultContentType = "application/json; charset=utf-8";

    readonly JsonSerializerSettings _settings;
    readonly Encoding _encoding;

    public JsonSerializer(JsonSerializerSettings settings, Encoding encoding)
    {
        _encoding = encoding ?? Encoding.UTF8;
        _settings = settings ?? new JsonSerializerSettings();

        if (_encoding.WebName != "utf-8")
        {
            throw new ArgumentException("Only UTF-8 for now, sorry");
        }
    }

    public TransportMessage Serialize(LogicalMessage message)
    {
        var headers = message.Headers.Clone();
        var body = message.Body;

        headers[ToposHeaders.ContentType] = DefaultContentType;
        headers[ToposHeaders.MessageType] = body.GetType().GetSimpleAssemblyQualifiedTypeName();

        var json = JsonConvert.SerializeObject(body, _settings);
        var bytes = _encoding.GetBytes(json);

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

        var bytes = message.Body;
        var json = _encoding.GetString(bytes);

        GetValue(headers, ToposHeaders.MessageType, out var messageType);

        try
        {
            var type = messageType.ParseType();
            var body = JsonConvert.DeserializeObject(json, type, _settings);
            var position = message.Position;
            return new ReceivedLogicalMessage(headers, body, position);
        }
        catch (Exception exception)
        {
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