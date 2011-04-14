using System;
using System.IO;
using Microsoft.Ajax.Utilities;

namespace Knapsack
{
    public class ModuleWriter
    {
        readonly TextWriter textWriter;
        readonly Func<string, string> loadSourceFromFile;

        public ModuleWriter(TextWriter textWriter, Func<string, string> loadSourceFromFile)
        {
            this.textWriter = textWriter;
            this.loadSourceFromFile = loadSourceFromFile;
        }

        public void Write(Module module)
        {
            var minifier = new Minifier();
            var first = true;
            foreach (var script in module.Scripts)
            {
                if (!first) textWriter.WriteLine();
                first = false;

                var source = loadSourceFromFile(script.Path);
                var minifiedSource = minifier.MinifyJavaScript(source);
                textWriter.Write(minifiedSource);
            }
        }
    }
}
