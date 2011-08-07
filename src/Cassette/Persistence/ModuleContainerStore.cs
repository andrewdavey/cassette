using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Cassette.Persistence
{
    public class ModuleContainerStore<T> : IModuleContainerStore<T>
        where T : Module
    {
        public ModuleContainerStore(IFileSystem fileSystem, IModuleFactory<T> moduleFactory)
        {
            this.fileSystem = fileSystem;
            this.moduleFactory = moduleFactory;
        }

        readonly IFileSystem fileSystem;
        readonly IModuleFactory<T> moduleFactory;

        public IModuleContainer<T> Load()
        {
            var containerXmlFilename = GetContainerXmlFilename();
            if (fileSystem.FileExists(containerXmlFilename) == false)
            {
                return EmptyContainer();
            }

            var containerElement = LoadContainerElement(containerXmlFilename);
            var lastWriteTime = new DateTime(long.Parse(containerElement.Attribute("lastWriteTime").Value));
            var moduleElements = containerElement.Elements("module");
            var modules = CreateModules(moduleElements);

            return new ModuleContainer<T>(modules, lastWriteTime, "");
        }

        IModuleContainer<T> EmptyContainer()
        {
            return new ModuleContainer<T>(Enumerable.Empty<T>(), DateTime.MinValue, "");
        }

        XElement LoadContainerElement(string filename)
        {
            using (var containerFile = fileSystem.OpenRead(filename))
            {
                return XDocument.Load(containerFile).Root;
            }
        }

        string GetContainerXmlFilename()
        {
            return "container.xml";
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

        public void Save(IModuleContainer<T> moduleContainer)
        {
            SaveContainerXml(moduleContainer);
            foreach (var module in moduleContainer)
            {
                SaveModule(module, moduleContainer);
            }
        }

        void SaveContainerXml(IModuleContainer<T> moduleContainer)
        {
            var createManifestVisitor = new CreateManifestVisitor();
            var xml = new XDocument(
                new XElement("container",
                    new XAttribute("lastWriteTime", moduleContainer.LastWriteTime.Ticks),
                    moduleContainer.Select(createManifestVisitor.CreateManifest)
                )
            );
            using (var fileStream = fileSystem.OpenWrite(GetContainerXmlFilename()))
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
            var filename = module.Directory + ".module";
            using (var fileStream = fileSystem.OpenWrite(filename))
            {
                using (var dataStream = module.Assets[0].OpenStream())
                {
                    dataStream.CopyTo(fileStream);
                }
                fileStream.Flush();
            }
        }
    }
}
