using System;
using System.Linq;
using System.IO;
using Microsoft.Ajax.Utilities;
using Jurassic;
using Jurassic.Library;

namespace Knapsack
{
    public class ModuleWriter
    {
        readonly TextWriter textWriter;
        readonly Func<string, string> loadSourceFromFile;
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

        public ModuleWriter(TextWriter textWriter, Func<string, string> loadSourceFromFile, ICoffeeScriptCompiler coffeeScriptCompiler)
        {
            this.textWriter = textWriter;
            this.loadSourceFromFile = loadSourceFromFile;
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        public void Write(Module module)
        {
            var minifier = new Minifier();

            textWriter.Write(
                minifier.MinifyJavaScript(
                    string.Join(
                        "\r\n",
                        module.Scripts.Select(s => Process(s.Path))
                    )
                )
            );
        }

        string Process(string scriptPath)
        {
            if (scriptPath.EndsWith(".coffee", StringComparison.OrdinalIgnoreCase))
            {
                return coffeeScriptCompiler.CompileCoffeeScript(scriptPath);
            }
            else
            {
                return loadSourceFromFile(scriptPath);
            }
        }

    }
}
