using System;

namespace Cassette.Manifests
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