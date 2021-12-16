using System;
using System.Linq;
using System.Text;
using Topos.Extensions;

namespace Topos.Serialization;

public class Utf8StringEncoder : IMessageSerializer
{
    const string ContentType = "text/plain;charset=utf-8";
    static readonly Encoding Encoding = Encoding.UTF8;

    public TransportMessage Serialize(LogicalMessage message)
    {
        var headers = message.Headers.Clone();
        var body = message.Body;

        if (!(body is string str))
        {
            throw new ArgumentException($"Invalid message body: {body} – UTF8 string encoder can only serialize messages of type System.String!");
        }

        headers[ToposHeaders.ContentType] = ContentType;

        return new TransportMessage(headers, Encoding.GetBytes(str));
    }

    public ReceivedLogicalMessage Deserialize(ReceivedTransportMessage message)
    {
        var headers = message.Headers.Clone();
        var body = message.Body;

        if (!headers.TryGetValue(ToposHeaders.ContentType, out var contentType))
        {
            throw new ArgumentException($"Could not find '{ToposHeaders.ContentType}' header on the incoming message");
        }

        // quick path
        if (contentType == ContentType)
        {
            return new ReceivedLogicalMessage(headers, Encoding.GetString(body), message.Position);
        }

        // otherwise, we need to parse the content type
        var parts = contentType.Split(';').Select(t => t.Trim()).ToArray();

        if (parts.Length != 2)
        {
            throw new ArgumentException($"Expected exactly 2 parts separated by ';' in content type, got this: '{contentType}'");
        }

        if (parts[0] != "text/plain")
        {
            throw new ArgumentException($"Invalid content type '{contentType}', expected 'text/plain' and then an encoding, e.g. like '{ContentType}'");
        }

        var encoding = GetEncoding(parts[1], contentType);

        return new ReceivedLogicalMessage(headers, encoding.GetString(body), message.Position);
    }

    static Encoding GetEncoding(string encodingName, string contentType)
    {
        try
        {
            return Encoding.GetEncoding(encodingName);
        }
        catch (Exception exception)
        {
            throw new ArgumentException($"Invalid encoding '{encodingName}' found in content type '{contentType}'", exception);
        }
    }
}