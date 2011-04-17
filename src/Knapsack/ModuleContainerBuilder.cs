using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Knapsack
{
    public class ModuleContainerBuilder
    {
        public ModuleContainerBuilder(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;

            var last = rootDirectory.Last();
            if (last != Path.DirectorySeparatorChar && last != Path.AltDirectorySeparatorChar)
            {
                rootDirectory += "/";
            }
        }

        readonly string rootDirectory;
        readonly List<string> relativeModuleDirectories = new List<string>();

        public void AddModule(string relativeDirectory)
        {
            relativeModuleDirectories.Add(relativeDirectory.Replace('\\', '/'));
        }

        public void AddModuleForEachSubdirectoryOf(string directory)
        {
            var fullPath = rootDirectory + directory;
            foreach (var path in Directory.GetDirectories(fullPath))
            {
                AddModule(path.Substring(rootDirectory.Length));
            }
        }

        public ModuleContainer Build()
        {
            var modules = relativeModuleDirectories.Select(LoadUnresolvedModule);
            return new ModuleContainer(UnresolvedModule.ResolveAll(modules));
        }

        UnresolvedModule LoadUnresolvedModule(string relativeDirectory)
        {
            var path = rootDirectory + relativeDirectory;
            var filenames = Directory.GetFiles(path, "*.js")
                .Where(f => !f.EndsWith("-vsdoc.js"))
                .Select(f => f.Replace('\\', '/'));
            return new UnresolvedModule(relativeDirectory, filenames.Select(LoadScript).ToArray());
        }

        UnresolvedScript LoadScript(string filename)
        {
            var parser = new ScriptParser();
            using (var fileStream = File.OpenRead(filename))
            {
                return parser.Parse(fileStream, filename.Substring(rootDirectory.Length));
            }
        }
    }
}
