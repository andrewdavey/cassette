using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System;
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
            this.rootDirectory = rootDirectory;
            this.coffeeScriptCompiler = coffeeScriptCompiler;

            rootDirectory = EnsureRootDirectoryEndsWithSlash(rootDirectory);
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

        public void AddModule(string relativeModuleDirectory)
        {
            relativeModuleDirectories.Add(relativeModuleDirectory.Replace('\\', '/'));
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
            var moduleBuilder = new UnresolvedModuleBuilder(rootDirectory);
            var unresolvedModules = relativeModuleDirectories.Select(moduleBuilder.Build);
            var modules = UnresolvedModule.ResolveAll(unresolvedModules);
            return new ModuleContainer(modules, storage, rootDirectory, coffeeScriptCompiler);
        }
    }
}
