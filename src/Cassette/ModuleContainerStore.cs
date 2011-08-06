using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Cassette
{
    public class ModuleContainerStore<T> : IModuleContainerStore<T>
        where T : Module
    {
        public ModuleContainerStore(Func<string, Stream> openFile, IModuleFactory<T> moduleFactory)
        {
            this.openFile = openFile;
            this.moduleFactory = moduleFactory;
        }

        readonly Func<string, Stream> openFile;
        readonly IModuleFactory<T> moduleFactory;

        public IModuleContainer<T> Load()
        {
            var containerElement = LoadContainerElement();
            if (containerElement == null) return EmptyContainer();
            var rootDirectory = containerElement.Attribute("rootDirectory").Value;
            var lastWriteTime = new DateTime(long.Parse(containerElement.Attribute("lastWriteTime").Value));
            var moduleElements = containerElement.Elements("module");
            var modules = CreateModules(rootDirectory, moduleElements);

            return new ModuleContainer<T>(modules, lastWriteTime);
        }

        public void Save(IModuleContainer<T> moduleContainer)
        {
            throw new NotImplementedException();
        }

        IModuleContainer<T> EmptyContainer()
        {
            return new ModuleContainer<T>(Enumerable.Empty<T>(), DateTime.MinValue);
        }

        XElement LoadContainerElement()
        {
            using (var containerFile = openFile(Path.Combine(typeof(T).Name + "s", "container.xml")))
            {
                if (containerFile == null) return null;
                return XDocument.Load(containerFile).Root;
            }
        }

        T[] CreateModules(string rootDirectory, IEnumerable<XElement> moduleElements)
        {
            return (
                from moduleElement in moduleElements
                select CreateModule(rootDirectory, moduleElement)
            ).ToArray();
        }

        T CreateModule(string rootDirectory, XElement moduleElement)
        {
            var directory = moduleElement.Attribute("directory").Value;
            var module = moduleFactory.CreateModule(Path.Combine(rootDirectory, directory));

            var singleAsset = CreateSingleAssetForModule(moduleElement, module, GetPersistedAssetFilename(directory), rootDirectory);
            module.Assets.Add(singleAsset);
            return module;
        }

        CachedAsset CreateSingleAssetForModule(XElement moduleElement, T module, string moduleFilename, string rootDirectory)
        {
            var assetInfos = CreateAssetInfos(moduleElement, module.Directory);
            var persistedAssetFilename = moduleFilename;
            var asset = new CachedAsset(assetInfos, () => openFile(persistedAssetFilename));
            AddReferencesToAsset(asset, moduleElement, rootDirectory);
            return asset;
        }

        void AddReferencesToAsset(IAsset asset, XElement moduleElement, string rootDirectory)
        {
            var modulePaths =
                from element in moduleElement.Elements("reference")
                let modulePath = element.Attribute("module").Value
                select Path.Combine(rootDirectory, modulePath);
            foreach (var path in modulePaths)
            {
                asset.AddReference(path, 0);
            }
        }

        string GetPersistedAssetFilename(string directoryName)
        {
            return Path.Combine(typeof(T).Name + "s", directoryName);
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
