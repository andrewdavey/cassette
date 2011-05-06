using System;
using System.Linq;
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

            textWriter.Write(
                minifier.MinifyJavaScript(
                    string.Join(
                        "\r\n",
                        module.Scripts.Select(s => loadSourceFromFile(s.Path))
                    )
                )
            );
        }
    }
}
