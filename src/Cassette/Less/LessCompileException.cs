using System;

namespace Cassette.Less
{
    public class LessCompileException : Exception
    {
        public LessCompileException(string message)
            : base(message)
        {
        }
    }
}
