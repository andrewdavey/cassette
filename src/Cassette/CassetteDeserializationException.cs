using System;
using System.Runtime.Serialization;

namespace Cassette
{
    [Serializable]
    class CassetteDeserializationException : Exception
    {
        public CassetteDeserializationException(string message) : base(message)
        {   
        }

        public CassetteDeserializationException(string message, Exception innerException) : base(message, innerException)
        {   
        }

        protected CassetteDeserializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}