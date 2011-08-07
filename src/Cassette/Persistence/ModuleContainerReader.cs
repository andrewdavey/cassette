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
        public ModuleContainerReader(IFileSystem fileSystem, IModuleFactory<T> moduleFactory)
        {
            this.fileSystem = fileSystem;
            this.moduleFactory = moduleFactory;
        }

        readonly IFileSystem fileSystem;
        readonly IModuleFactory<T> moduleFactory;

        public IModuleContainer<T> Load()
        {
            if (fileSystem.FileExists("container.xml") == false)
            {
                return EmptyContainer();
            }

            var containerElement = LoadContainerElement();
            var lastWriteTime = new DateTime(long.Parse(containerElement.Attribute("lastWriteTime").Value));
            var moduleElements = containerElement.Elements("module");
            var modules = CreateModules(moduleElements);

            return new ModuleContainer<T>(modules, lastWriteTime);
        }

        IModuleContainer<T> EmptyContainer()
        {
            return new ModuleContainer<T>(Enumerable.Empty<T>(), DateTime.MinValue);
        }

        XElement LoadContainerElement()
        {
            using (var containerFile = fileSystem.OpenRead("container.xml"))
            {
                return XDocument.Load(containerFile).Root;
            }
        }

        T[] CreateModules(IEnumerable<XElement> moduleElements)
        {
            return (
                from moduleElement in moduleElements
                select CreateModule(moduleElement)
            ).ToArray();
        }

        T CreateModule(XElement moduleElement)
        {
            var directory = moduleElement.Attribute("directory").Value;
            var filename = directory + ".module";
            var module = moduleFactory.CreateModule(directory);

            var singleAsset = CreateSingleAssetForModule(moduleElement, module, filename);
            module.Assets.Add(singleAsset);
            return module;
        }

        CachedAsset CreateSingleAssetForModule(XElement moduleElement, T module, string moduleFilename)
        {
            var assetInfos = CreateAssetInfos(moduleElement, module.Directory);
            var asset = new CachedAsset(assetInfos, () => fileSystem.OpenRead(moduleFilename));
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

        IEnumerable<AssetInfo> CreateAssetInfos(XElement moduleElement, string directory)
        {
            return moduleElement.Elements("asset").Select(a => CreateAssetInfo(a, directory));
        }

        AssetInfo CreateAssetInfo(XElement assetElement, string directory)
        {
            var relativeFilename = assetElement.Attribute("filename").Value;
            var absoluteFilename = Path.Combine(directory, relativeFilename);
            return new AssetInfo(absoluteFilename);
        }
    }
}
