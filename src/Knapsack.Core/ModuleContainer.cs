using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Knapsack.CoffeeScript;

namespace Knapsack
{
    public class ModuleContainer
    {
        readonly Module[] modules;
        readonly ModuleManifest manifest;
        readonly IsolatedStorageFile storage;
        readonly string rootDirectory;
        readonly Dictionary<string, Module> modulesByScriptPath;
        readonly StringComparer pathComparer = StringComparer.OrdinalIgnoreCase;
        readonly ICoffeeScriptCompiler coffeeScriptCompiler;

        public ModuleContainer(IEnumerable<Module> modules, IsolatedStorageFile storage, string rootDirectory, ICoffeeScriptCompiler coffeeScriptCompiler)
        {
            this.modules = modules.ToArray();
            this.manifest = new ModuleManifest(this.modules);
            this.storage = storage;
            this.rootDirectory = rootDirectory;
            this.coffeeScriptCompiler = coffeeScriptCompiler;

            modulesByScriptPath = (
                from module in this.modules
                from script in module.Scripts
                select new { script.Path, module }
            ).ToDictionary(x => x.Path, x => x.module, pathComparer);
        }

        public Module FindModuleContainingScript(string scriptPath)
        {
            Module module;
            if (modulesByScriptPath.TryGetValue(scriptPath, out module))
            {
                return module;
            }

            return null;
        }

        public bool Contains(string modulePath)
        {
            return modules.Any(m => pathComparer.Equals(m.Path, modulePath));
        }

        public Module FindModule(string modulePath)
        {
            return modules.FirstOrDefault(m => pathComparer.Equals(m.Path, modulePath));
        }

        public Stream OpenModuleFile(Module module)
        {
            return storage.OpenFile(module.Hash.ToHexString() + ".js", FileMode.Open, FileAccess.Read);
        }

        public void UpdateStorage()
        {
            var cachedManifest = ReadManifestFromStorage() ?? CreateEmptyManifest();
            var differences = manifest.CompareTo(cachedManifest);

            if (differences.Any())
            {
                ApplyDifferencesToStorage(differences);
                WriteManifestToStorage();
            }
        }

        ModuleManifest ReadManifestFromStorage()
        {
            if (!storage.FileExists("manifest.xml")) return null;

            using (var stream = storage.OpenFile("manifest.xml", FileMode.Open, FileAccess.Read))
            {
                var reader = new ModuleManifestReader(stream);
                return reader.Read();
            }
        }

        ModuleManifest CreateEmptyManifest()
        {
            return new ModuleManifest(Enumerable.Empty<Module>());
        }


        void WriteManifestToStorage()
        {
            using (var stream = storage.OpenFile("manifest.xml", FileMode.Create, FileAccess.Write))
            {
                var writer = new ModuleManifestWriter(stream);
                writer.Write(manifest);
            }
        }

        void ApplyDifferencesToStorage(ModuleDifference[] differences)
        {
            foreach (var difference in differences)
            {
                switch (difference.Type)
                {
                    case ModuleDifferenceType.Changed:
                    case ModuleDifferenceType.Added:
                        WriteModuleToStorage(difference.Module);
                        break;

                    case ModuleDifferenceType.Deleted:
                        DeleteModuleFromStorage(difference.Module);
                        break;
                }
            }
        }

        void WriteModuleToStorage(Module module)
        {
            var filename = module.Hash.ToHexString() + ".js";
            storage.CreateDirectory(Path.GetDirectoryName(filename));
            using (var stream = storage.OpenFile(filename, FileMode.Create, FileAccess.Write))
            using (var textWriter = new StreamWriter(stream))
            {
                var writer = new ModuleWriter(textWriter, LoadSourceFromFile, coffeeScriptCompiler);
                writer.Write(module);
            }
        }

        void DeleteModuleFromStorage(Module module)
        {
            var filename = module.Hash.ToHexString() + ".js";
            if (storage.FileExists(filename))
            {
                storage.DeleteFile(filename);
            }
        }

        string LoadSourceFromFile(string relativeFilename)
        {
            var fullPath = Path.Combine(rootDirectory, relativeFilename);
            return File.ReadAllText(fullPath);
        }

    }
}
