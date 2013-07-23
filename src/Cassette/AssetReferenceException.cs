using System;
using System.Runtime.Serialization;

namespace Cassette
{
    [Serializable]
    public class AssetReferenceException : Exception
    {
        public AssetReferenceException(string message) : base(message)
        {
        }

        public AssetReferenceException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }

        protected AssetReferenceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

