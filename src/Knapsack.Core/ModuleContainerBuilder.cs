using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System;

namespace Knapsack
{
    public class ModuleContainerBuilder
    {
        public ModuleContainerBuilder(IsolatedStorageFile storage, string rootDirectory, ICoffeeScriptCompiler coffeeScriptCompiler)
        {
            this.storage = storage;
            this.rootDirectory = rootDirectory;
            this.coffeeScriptCompiler = coffeeScriptCompiler;

            var last = rootDirectory.Last();
            if (last != Path.DirectorySeparatorChar && last != Path.AltDirectorySeparatorChar)
            {
                rootDirectory += "/";
            }
        }

        readonly IsolatedStorageFile storage;
        readonly string rootDirectory;
        readonly List<string> relativeModuleDirectories = new List<string>();
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

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
            return new ModuleContainer(UnresolvedModule.ResolveAll(modules), storage, rootDirectory, coffeeScriptCompiler);
        }

        UnresolvedModule LoadUnresolvedModule(string relativeDirectory)
        {
            var path = rootDirectory + relativeDirectory;
            var filenames = Directory.GetFiles(path, "*.js").Concat(Directory.GetFiles(path, "*.coffee"))
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
