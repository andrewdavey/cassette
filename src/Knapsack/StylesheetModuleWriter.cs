using System;
using System.IO;
using System.Linq;
using Microsoft.Ajax.Utilities;

namespace Knapsack
{
    public class StylesheetModuleWriter : IModuleWriter
    {
        readonly TextWriter textWriter;
        readonly string rootDirectory;
        readonly Func<string, string> loadSourceFromFile;

        public StylesheetModuleWriter(TextWriter textWriter, string rootDirectory, Func<string, string> loadSourceFromFile)
        {
            this.textWriter = textWriter;
            this.rootDirectory = rootDirectory;
            this.loadSourceFromFile = loadSourceFromFile;
        }

        public void Write(Module module)
        {
            var minifier = new Minifier();

            textWriter.Write(
                minifier.MinifyStyleSheet(
                    string.Join(
                        "\r\n",
                        module.Resources.Select(s => loadSourceFromFile(rootDirectory + s.Path))
                    )
                )
            );
        }
    }
}
