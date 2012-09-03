using System;

namespace Cassette.Spriting.Spritastic
{
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

        public string CssRule { get; private set; }
    }
}