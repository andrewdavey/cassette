using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml.Linq;

namespace Knapsack
{
    /// <summary>
    /// Maintains a cache of module files in isolated storage.
    /// </summary>
    public class ModuleCache
    {
        readonly IsolatedStorageFile storage;
        readonly string rootDirectory;

        public ModuleCache(IsolatedStorageFile storage, string rootDirectory)
        {
            this.storage = storage;
            this.rootDirectory = rootDirectory;
        }

        public void UpdateFrom(ModuleContainer moduleContainer)
        {
            ModuleContainer cachedContainer = LoadCachedContainer();

            var differences = moduleContainer.CompareTo(cachedContainer);

            if (differences.Any())
            {
                ApplyDifferencesToCache(differences);
                WriteManifest(moduleContainer);
            }
        }

        public Stream OpenModuleFile(Module module)
        {
            storage.OpenFile(module.Path, FileMode.Open, FileAccess.Read);
        }

        void ApplyDifferencesToCache(ModuleDifference[] differences)
        {
            foreach (var difference in differences)
            {
                switch (difference.Type)
                {
                    case ModuleDifferenceType.Changed:
                    case ModuleDifferenceType.Added:
                        WriteModule(difference.Module);
                        break;

                    case ModuleDifferenceType.Deleted:
                        DeleteModule(difference.Module);
                        break;
                }
            }
        }

        void WriteModule(Module module)
        {
            using (var stream = storage.OpenFile(module.Path, FileMode.Create, FileAccess.Write))
            using (var textWriter = new StreamWriter(stream))
            {
                var writer = new ModuleWriter(textWriter, LoadSourceFromFile);
                writer.Write(module);
            }
        }

        string LoadSourceFromFile(string relativeFilename)
        {
            var fullPath = Path.Combine(rootDirectory, relativeFilename);
            return File.ReadAllText(fullPath);
        }

        void DeleteModule(Module module)
        {
            storage.DeleteDirectory(module.Path);
        }

        void WriteManifest(ModuleContainer moduleContainer)
        {
            using (var stream = storage.OpenFile("manifest.xml", FileMode.Create, FileAccess.Write))
            {
                CreateManifestXml(moduleContainer).Save(stream);
            }
        }

        ModuleContainer ReadManifest()
        {
            if (!storage.FileExists("manifest.xml")) return new ModuleContainer(Enumerable.Empty<Module>());

            using (var stream = storage.OpenFile("manifest.xml", FileMode.Open, FileAccess.Read))
            {
                var document = XDocument.Load(stream);
                var moduleElements = document.Root.Elements("module");
                return new ModuleContainer(
                    moduleElements.Select(ReadModuleElement)
                );
            }
        }

        Module ReadModuleElement(XElement moduleElement)
        {
            return new Module(
                moduleElement.Attribute("path").Value,
                moduleElement.Elements("script").Select(ReadScriptElement).ToArray(),
                moduleElement.Elements("reference").Select(ReadReferenceElement).ToArray()
            );
        }

        Script ReadScriptElement(XElement element)
        {
            return new Script(
                element.Attribute("path").Value,
                ByteArrayExtensions.FromHexString(element.Attribute("hash").Value),
                new string[0]
            );
        }

        string ReadReferenceElement(XElement element)
        {
            return element.Attribute("path").Value;
        }

        XDocument CreateManifestXml(ModuleContainer moduleContainer)
        {
            return new XDocument(
                new XElement("manifest",
                    from module in moduleContainer.Modules
                    select new XElement("module",
                        new XAttribute("path", module.Path),
                        from script in module.Scripts
                        select new XElement("script",
                            new XAttribute("path", script.Path),
                            new XAttribute("hash", script.Hash.ToHexString())
                        )
                    )
                )
            );
        }

        ModuleContainer LoadCachedContainer()
        {
            throw new NotImplementedException();
        }

    }
}
