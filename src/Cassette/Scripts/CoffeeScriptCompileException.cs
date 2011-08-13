using System;

namespace Cassette.Scripts
{
    public class CoffeeScriptCompileException : Exception
    {
        public CoffeeScriptCompileException(string message, string sourcePath) : base(message)
        {
            SourcePath = sourcePath;
        }

        public string SourcePath { get; private set; }
    }
}
