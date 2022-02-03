using System;

#if !NET_DOTS
using System.Runtime.Serialization;
#endif

namespace Unity.Serialization
{
    /// <summary>
    /// The exception that is thrown when trying to parse a value as an actual type.
    /// </summary>
    [Serializable]
    public class ParseErrorException : Exception
    {
        /// <summary>
        /// Initialized a new instance of the <see cref="ParseErrorException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ParseErrorException(string message) : base(message)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseErrorException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The inner exception reference.</param>
        public ParseErrorException(string message, Exception inner) : base(message, inner)
        {

        }

#if !NET_DOTS
        /// <summary>
        /// Initializes a new instance of the <see cref="ParseErrorException" /> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected ParseErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}
