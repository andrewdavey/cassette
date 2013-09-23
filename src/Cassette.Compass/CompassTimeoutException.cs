using System;
using System.Runtime.Serialization;

namespace Cassette.Compass
{
    [Serializable]
    public class CompassTimeoutException : Exception
    {
        public CompassTimeoutException()
        {
        }

        public CompassTimeoutException(string message) : base(message)
        {
        }

        public CompassTimeoutException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CompassTimeoutException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
