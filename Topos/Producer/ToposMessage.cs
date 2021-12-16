using System.Collections.Generic;

namespace Topos.Producer;

/// <summary>
/// An envelope for a message ("body"), and maybe some headers
/// </summary>
public class ToposMessage
{
    /// <summary>
    /// Gets the body of this message
    /// </summary>
    public object Body { get; }

    /// <summary>
    /// Gets the headers of this message, or null if none were provided
    /// </summary>
    public IDictionary<string, string> Headers { get; }

    /// <summary>
    /// Constructs this <see cref="ToposMessage"/> with the given <paramref name="body"/> and no headers
    /// </summary>
    public ToposMessage(object body) : this(body, null) { }

    /// <summary>
    /// Constructs this <see cref="ToposMessage"/> with the given <paramref name="body"/> and <paramref name="headers"/>
    /// </summary>
    public ToposMessage(object body, IDictionary<string, string> headers)
    {
        Body = body;
        Headers = headers;
    }
}