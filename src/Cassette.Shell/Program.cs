using System;
using System.IO;
using System.Linq;
using Cassette.CoffeeScript;
using Cassette.Assets.Scripts;

namespace Cassette.Shell
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: cassette {path} {output-directory}");
                return;
            }
            var path = args[0];
            if (path.EndsWith("*"))
            {
                path = Path.GetFullPath(path.Substring(0, path.Length - 2)) + "\\";
                var builder = new ScriptModuleContainerBuilder(null, path, new CoffeeScriptCompiler(File.ReadAllText));
                builder.AddModuleForEachSubdirectoryOf("", "");
                var container = builder.Build();
                foreach (var module in container)
                {
                    var outputFilename = Path.GetFullPath(Path.Combine(args[1], module.Path + ".js"));
                    using (var file = new StreamWriter(outputFilename))
                    {
                        var writer = new ScriptModuleWriter(file, path, File.ReadAllText, new CoffeeScriptCompiler(File.ReadAllText));
                        writer.Write(module);
                        file.Flush();
                    }
                }
            }
            else
            {
                path = Path.GetFullPath(path);
                var builder = new UnresolvedScriptModuleBuilder(path);
                var unresolvedModule = builder.Build("", null); // path is the module, so no extra path is required.
                var module = UnresolvedModule.ResolveAll(new[] { unresolvedModule }).First();

                var writer = new ScriptModuleWriter(Console.Out, path, File.ReadAllText, new CoffeeScriptCompiler(File.ReadAllText));
                writer.Write(module);
            }
        }

    }
}
