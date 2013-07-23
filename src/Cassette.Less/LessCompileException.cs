using System;
using System.Runtime.Serialization;

namespace Cassette.Stylesheets
{
    [Serializable]
    public class LessCompileException : Exception
    {
        public LessCompileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public LessCompileException(string message)
            : base(message)
        {
        }

        protected LessCompileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

