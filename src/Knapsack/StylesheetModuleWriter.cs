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
        readonly Func<string, string> readFileText;

        public StylesheetModuleWriter(TextWriter textWriter, string rootDirectory, Func<string, string> readFileText)
        {
            this.textWriter = textWriter;
            this.rootDirectory = rootDirectory;
            this.readFileText = readFileText;
        }

        public void Write(Module module)
        {
            var minifier = new Minifier();

            textWriter.Write(
                minifier.MinifyStyleSheet(
                    string.Join(
                        "\r\n",
                        module.Resources.Select(ReadCss)
                    )
                )
            );
        }

        string ReadCss(Resource resource)
        {
            var css = readFileText(rootDirectory + resource.Path);
            return css;
        }
    }
}
