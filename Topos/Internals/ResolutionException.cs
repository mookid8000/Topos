using System;
using System.Runtime.Serialization;

namespace Topos.Internals;

/// <inheritdoc />
/// <summary>
/// Exceptions that is thrown when something goes wrong while working with the injectionist
/// </summary>
[Serializable]
class ResolutionException : Exception
{
    /// <summary>
    /// Constructs the exception
    /// </summary>
    public ResolutionException(string message, string explanation = null)
        : base(GetMessage(message, explanation))
    {
    }

    /// <summary>
    /// Constructs the exception
    /// </summary>
    public ResolutionException(Exception innerException, string message, string explanation = null)
        : base(GetMessage(message, explanation), innerException)
    {
    }

    static string GetMessage(string message, string explanation)
    {
        return string.IsNullOrWhiteSpace(explanation)
            ? message
            : $@"{message}

{explanation}";
    }

    /// <summary>
    /// Constructs the exception
    /// </summary>
    public ResolutionException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}