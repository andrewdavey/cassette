using System;

namespace Cassette.Compilation
{
    public class LessCompileException : Exception
    {
        public LessCompileException(string message)
            : base(message)
        {
        }
    }
}
