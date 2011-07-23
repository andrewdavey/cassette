using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using Cassette.CoffeeScript;
using Cassette.Utilities;

namespace Cassette
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
                from asset in module.Assets
                select new { asset.Path, module }
            ).ToDictionary(x => x.Path, x => x.module, pathComparer);
        }

        public Module FindModuleContainingAsset(string assetPath)
        {
            Module module;
            if (modulesByScriptPath.TryGetValue(assetPath, out module))
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
            try
            {
                using (var stream = storage.OpenFile(manifestFilename, FileMode.Create, FileAccess.Write))
                {
                    var writer = new ModuleManifestWriter(stream);
                    writer.Write(manifest);
                }
            }
            catch (IOException)
            {
                storage.CreateFile(manifestFilename);
            }
        }

        void ApplyDifferencesToStorage(ModuleDifference[] differences)
        {
            foreach (var difference in differences)
            {
                switch (difference.Type)
                {
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
            var name = module.Path.Replace('\\', '_').Replace('/', '_');
            var hash = module.Hash.ToHexString();
            return name + "_" + hash;
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