using System;
using System.IO;
using System.Linq;

namespace Knapsack
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: knap {path}");
                return;
            }

            var path = Path.GetFullPath(args[0]);

            var scriptFilenames = Directory.GetFiles(path, "*.js", SearchOption.AllDirectories);
            var scripts = from filename in scriptFilenames
                          select LoadScript(filename);

            var unresolvedModule = new UnresolvedModule("", scripts.ToArray());
            var module = unresolvedModule.Resolve(_ => "");

            var writer = new ModuleWriter(Console.Out, p => File.ReadAllText(Path.Combine(path, p)));
            writer.Write(module);
        }

        static Script LoadScript(string filename)
        {
            var scriptParser = new ScriptParser();
            using (var stream = File.OpenRead(filename))
            {
                return scriptParser.Parse(stream, filename);
            }
        }
    }
}
