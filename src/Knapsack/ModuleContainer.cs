using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Knapsack.CoffeeScript;
using Knapsack.Utilities;

namespace Knapsack
{
    public class ModuleContainer : IEnumerable<Module>
    {
        readonly Module[] modules;
        readonly ModuleManifest manifest;
        readonly IsolatedStorageFile storage;
        readonly Func<TextWriter, IModuleWriter> createModuleWriter;
        readonly Dictionary<string, Module> modulesByScriptPath;
        readonly StringComparer pathComparer = StringComparer.OrdinalIgnoreCase;

        public ModuleContainer(IEnumerable<Module> modules, IsolatedStorageFile storage, Func<TextWriter, IModuleWriter> createModuleWriter)
        {
            this.modules = modules.ToArray();
            this.manifest = new ModuleManifest(this.modules);
            this.storage = storage;
            this.createModuleWriter = createModuleWriter;

            modulesByScriptPath = (
                from module in this.modules
                from resource in module.Resources
                select new { resource.Path, module }
            ).ToDictionary(x => x.Path, x => x.module, pathComparer);
        }

        public Module FindModuleContainingResource(string resourcePath)
        {
            Module module;
            if (modulesByScriptPath.TryGetValue(resourcePath, out module))
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
            return storage.OpenFile(ModuleFilename(module), FileMode.Open, FileAccess.Read);
        }

        public void UpdateStorage(string manifestFilename)
        {
            var cachedManifest = ReadManifestFromStorage(manifestFilename) ?? CreateEmptyManifest();
            var differences = manifest.CompareTo(cachedManifest);

            if (differences.Length > 0)
            {
                ApplyDifferencesToStorage(differences);
                WriteManifestToStorage(manifestFilename);
            }
        }

        ModuleManifest ReadManifestFromStorage(string manifestFilename)
        {
            if (!storage.FileExists(manifestFilename)) return null;

            using (var stream = storage.OpenFile(manifestFilename, FileMode.Open, FileAccess.Read))
            {
                var reader = new ModuleManifestReader(stream);
                return reader.Read();
            }
        }

        ModuleManifest CreateEmptyManifest()
        {
            return new ModuleManifest(Enumerable.Empty<Module>());
        }

        void WriteManifestToStorage(string manifestFilename)
        {
            using (var stream = storage.OpenFile(manifestFilename, FileMode.Create, FileAccess.Write))
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
            var filename = ModuleFilename(module);
            using (var stream = storage.OpenFile(filename, FileMode.Create, FileAccess.Write))
            using (var textWriter = new StreamWriter(stream))
            {
                var writer = createModuleWriter(textWriter);
                writer.Write(module);
            }
        }

        void DeleteModuleFromStorage(Module module)
        {
            var filename = ModuleFilename(module);
            if (storage.FileExists(filename))
            {
                storage.DeleteFile(filename);
            }
        }

        string ModuleFilename(Module module)
        {
            return module.Hash.ToHexString();
        }

        public IEnumerator<Module> GetEnumerator()
        {
            foreach (var module in modules) yield return module;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return modules.GetEnumerator();
        }
    }
}