using System;
using System.Runtime.Serialization;

namespace Cassette.Scripts
{
    [Serializable]
    public class CoffeeScriptCompileException : Exception
    {
        public CoffeeScriptCompileException(string message, string sourcePath, Exception innerException) : base(message, innerException)
        {
            SourcePath = sourcePath;
        }

        protected CoffeeScriptCompileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string SourcePath { get; private set; }
    }
}