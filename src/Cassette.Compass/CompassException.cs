using System;
using System.Runtime.Serialization;

namespace Cassette.Compass
{
    [Serializable]
    public class CompassException : Exception
    {
        public CompassException()
        {
        }

        public CompassException(string message) : base(message)
        {
        }

        public CompassException(string message, Exception inner) : base(message, inner)
        {
        }

        protected CompassException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
