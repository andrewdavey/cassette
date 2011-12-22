using System;

namespace Cassette.Scripts
{
    public class CoffeeScriptCompileException : Exception
    {
        public CoffeeScriptCompileException(string message, string sourcePath, Exception innerException) : base(message, innerException)
        {
            SourcePath = sourcePath;
        }

        public string SourcePath { get; private set; }
    }
}