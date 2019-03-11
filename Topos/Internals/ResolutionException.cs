using System;
using System.Runtime.Serialization;

namespace Topos.Internals
{
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
        public ResolutionException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructs the exception
        /// </summary>
        public ResolutionException(Exception innerException, string message)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructs the exception
        /// </summary>
        public ResolutionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}