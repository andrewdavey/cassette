using System;

namespace Cassette
{
    class CassetteDeserializationException : Exception
    {
        public CassetteDeserializationException(string message) : base(message)
        {   
        }

        public CassetteDeserializationException(string message, Exception innerException) : base(message, innerException)
        {   
        }
    }
}