using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Knapsack.CoffeeScript;

namespace Knapsack
{
    public class ModuleContainerBuilder
    {
        readonly IsolatedStorageFile storage;
        readonly string rootDirectory;
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;
        readonly List<string> relativeModuleDirectories = new List<string>();

        public ModuleContainerBuilder(IsolatedStorageFile storage, string rootDirectory, ICoffeeScriptCompiler coffeeScriptCompiler)
        {
            this.storage = storage;
            this.rootDirectory = EnsureRootDirectoryEndsWithSlash(rootDirectory);
            this.coffeeScriptCompiler = coffeeScriptCompiler;
        }

        string EnsureRootDirectoryEndsWithSlash(string rootDirectory)
        {
            var last = rootDirectory.Last();
            if (last != Path.DirectorySeparatorChar && last != Path.AltDirectorySeparatorChar)
            {
                rootDirectory += "/";
            }
            return rootDirectory;
        }

        public void AddModule(string directoryRelativeToRootDirectory)
        {
            relativeModuleDirectories.Add(directoryRelativeToRootDirectory.Replace('\\', '/'));
        }

        public void AddModuleForEachSubdirectoryOf(string directoryRelativeToRootDirectory)
        {
            var fullPath = rootDirectory + directoryRelativeToRootDirectory;
            foreach (var path in Directory.GetDirectories(fullPath))
            {
                AddModule(path.Substring(rootDirectory.Length));
            }
        }

        public ModuleContainer Build()
        {
            var moduleBuilder = new UnresolvedModuleBuilder(rootDirectory);
            var unresolvedModules = relativeModuleDirectories.Select(moduleBuilder.Build);
            var modules = UnresolvedModule.ResolveAll(unresolvedModules);
            return new ModuleContainer(modules, storage, rootDirectory, coffeeScriptCompiler);
        }
    }
}
