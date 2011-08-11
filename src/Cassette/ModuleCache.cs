using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.Persistence;

namespace Cassette
{
    public class ModuleCache<T> : IModuleCache<T>
        where T : Module
    {
        public ModuleCache(IFileSystem fileSystem, IModuleFactory<T> moduleFactory)
        {
            this.fileSystem = fileSystem;
            this.moduleFactory = moduleFactory;
        }

        readonly IFileSystem fileSystem;
        readonly IModuleFactory<T> moduleFactory;
        readonly string containerFilename = "container.xml";

        public bool IsUpToDate(DateTime dateTime)
        {
            if (!fileSystem.FileExists(containerFilename)) return false;
            var lastWriteTime = fileSystem.GetLastWriteTimeUtc(containerFilename);
            return lastWriteTime >= dateTime;
        }

        public IModuleContainer<T> LoadModuleContainer()
        {
            var containerElement = LoadContainerElement(fileSystem);
            var moduleElements = containerElement.Elements("module");
            var modules = CreateModules(moduleElements, fileSystem);

            return new ModuleContainer<T>(modules);
        }

        IModuleContainer<T> EmptyContainer()
        {
            return new ModuleContainer<T>(Enumerable.Empty<T>());
        }

        XElement LoadContainerElement(IFileSystem fileSystem)
        {
            using (var containerFile = fileSystem.OpenFile(containerFilename, FileMode.Open, FileAccess.Read))
            {
                return XDocument.Load(containerFile).Root;
            }
        }

        T[] CreateModules(IEnumerable<XElement> moduleElements, IFileSystem fileSystem)
        {
            return (
                from moduleElement in moduleElements
                select CreateModule(moduleElement, fileSystem)
            ).ToArray();
        }

        T CreateModule(XElement moduleElement, IFileSystem fileSystem)
        {
            var directory = moduleElement.Attribute("directory").Value;
            var filename = FlattenPathToSingleFilename(directory) + ".module";
            var module = moduleFactory.CreateModule(directory);

            var singleAsset = CreateSingleAssetForModule(moduleElement, module, filename, fileSystem);
            module.Assets.Add(singleAsset);
            return module;
        }

        CachedAsset CreateSingleAssetForModule(XElement moduleElement, T module, string moduleFilename, IFileSystem fileSystem)
        {
            var assetInfos = CreateAssetInfos(moduleElement, module.Directory);
            var asset = new CachedAsset(assetInfos, () => fileSystem.OpenFile(moduleFilename, FileMode.Open, FileAccess.Read));
            AddReferencesToAsset(asset, moduleElement);
            return asset;
        }

        void AddReferencesToAsset(IAsset asset, XElement moduleElement)
        {
            var modulePaths =
                from element in moduleElement.Elements("reference")
                select element.Attribute("path").Value;

            foreach (var path in modulePaths)
            {
                asset.AddReference(path, 0);
            }
        }

        IEnumerable<CachedAssetSourceInfo> CreateAssetInfos(XElement moduleElement, string directory)
        {
            return moduleElement.Elements("asset").Select(a => CreateAssetInfo(a, directory));
        }

        CachedAssetSourceInfo CreateAssetInfo(XElement assetElement, string directory)
        {
            var relativeFilename = assetElement.Attribute("filename").Value;
            var absoluteFilename = Path.Combine(directory, relativeFilename);
            return new CachedAssetSourceInfo(absoluteFilename);
        }

        public void SaveModuleContainer(IModuleContainer<T> moduleContainer)
        {
            SaveContainerXml(moduleContainer);
            foreach (var module in moduleContainer.Modules)
            {
                SaveModule(module, moduleContainer);
            }
        }

        void SaveContainerXml(IModuleContainer<T> moduleContainer)
        {
            var createManifestVisitor = new CreateManifestVisitor();
            var xml = new XDocument(
                new XElement("container",
                    moduleContainer.Modules.Select(createManifestVisitor.CreateManifest)
                )
            );
            using (var fileStream = fileSystem.OpenFile(containerFilename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                xml.Save(fileStream);
            }
        }

        void SaveModule(T module, IModuleContainer<T> moduleContainer)
        {
            if (module.Assets.Count > 1)
            {
                throw new InvalidOperationException("Cannot save a module when assets have not been concatenated into a single asset.");
            }
            var filename = FlattenPathToSingleFilename(module.Directory) + ".module";
            using (var fileStream = fileSystem.OpenFile(filename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                if (module.Assets.Count > 0)
                {
                    using (var dataStream = module.Assets[0].OpenStream())
                    {
                        dataStream.CopyTo(fileStream);
                    }
                    fileStream.Flush();
                }
            }
        }

        string FlattenPathToSingleFilename(string path)
        {
            return path.Replace(Path.DirectorySeparatorChar, '`')
                       .Replace(Path.AltDirectorySeparatorChar, '`');
        }
    }
}
