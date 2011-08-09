using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Cassette.Persistence
{
    public class ModuleContainerReader<T> : IModuleContainerReader<T>
        where T : Module
    {
        public ModuleContainerReader(IModuleFactory<T> moduleFactory)
        {
            this.moduleFactory = moduleFactory;
        }

        readonly IModuleFactory<T> moduleFactory;

        public IModuleContainer<T> Load(IFileSystem fileSystem)
        {
            if (fileSystem.FileExists("container.xml") == false)
            {
                return EmptyContainer();
            }

            var containerElement = LoadContainerElement(fileSystem);
            var lastWriteTime = new DateTime(long.Parse(containerElement.Attribute("lastWriteTime").Value));
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
            using (var containerFile = fileSystem.OpenFile("container.xml", FileMode.Open, FileAccess.Read))
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
            var filename = directory + ".module";
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
                select element.Attribute("module").Value;

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
    }
}
