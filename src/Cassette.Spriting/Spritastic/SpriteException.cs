using System;
using System.Runtime.Serialization;

namespace Cassette.Spriting.Spritastic
{
    [Serializable]
    class SpriteException : ApplicationException
    {
        public SpriteException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public SpriteException(string cssRule, string message, Exception innerException) : base(message, innerException)
        {
            CssRule = cssRule;
        }

        protected SpriteException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string CssRule { get; private set; }
    }
}