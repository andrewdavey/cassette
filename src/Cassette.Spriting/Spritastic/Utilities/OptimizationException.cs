using System;
using System.Runtime.Serialization;

namespace Cassette.Spriting.Spritastic.Utilities
{
    [Serializable]
    class OptimizationException : Exception
    {
        public OptimizationException(string message) : base(message)
        {
        }

        public OptimizationException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }

        protected OptimizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}