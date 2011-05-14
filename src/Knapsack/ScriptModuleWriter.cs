using System;
using System.IO;
using System.Linq;
using Knapsack.CoffeeScript;
using Microsoft.Ajax.Utilities;

namespace Knapsack
{
    public class ScriptModuleWriter : IModuleWriter
    {
        readonly TextWriter textWriter;
        readonly string rootDirectory;
        readonly Func<string, string> loadSourceFromFile;
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

        public ScriptModuleWriter(TextWriter textWriter, string rootDirectory, Func<string, string> loadSourceFromFile, ICoffeeScriptCompiler coffeeScriptCompiler)
        {
            this.textWriter = textWriter;
            this.rootDirectory = rootDirectory;
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
                        module.Resources.Select(s => GetJavaScript(rootDirectory + s.Path))
                    )
                )
            );
        }

        string GetJavaScript(string scriptPath)
        {
            if (scriptPath.EndsWith(".coffee", StringComparison.OrdinalIgnoreCase))
            {
                return coffeeScriptCompiler.CompileFile(scriptPath);
            }
            else // assume it's a regular ".js" file
            {
                return loadSourceFromFile(scriptPath);
            }
        }

    }
}
