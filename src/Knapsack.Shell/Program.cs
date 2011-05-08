using System;
using System.IO;
using System.Linq;
using Knapsack.CoffeeScript;

namespace Knapsack.Shell
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: knapsack {path}");
                return;
            }

            var path = Path.GetFullPath(args[0]);

            var builder = new UnresolvedModuleBuilder(path);
            var unresolvedModule = builder.Build(""); // path is the module, so no extra path is required.
            var module = UnresolvedModule.ResolveAll(new[] { unresolvedModule }).First();

            var writer = new ModuleWriter(Console.Out, path, File.ReadAllText, new CoffeeScriptCompiler(File.ReadAllText));
            writer.Write(module);
        }

    }
}
