using System;

namespace Cassette.Stylesheets
{
    public class LessCompileException : Exception
    {
        public LessCompileException(string message)
            : base(message)
        {
        }
    }
}
