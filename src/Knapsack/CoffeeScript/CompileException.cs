using System;

namespace Knapsack.CoffeeScript
{
    public class CompileException : Exception
    {
        public CompileException(string message, string sourcePath) : base(message)
        {
            SourcePath = sourcePath;
        }

        public string SourcePath { get; private set; }
    }
}
