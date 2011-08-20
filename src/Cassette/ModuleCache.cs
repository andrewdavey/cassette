using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.Persistence;
using Cassette.Utilities;

namespace Cassette
{
    public class ModuleCache<T> : IModuleCache<T>
        where T : Module
    {
        public ModuleCache(IFileSystem cacheFileSystem, IModuleFactory<T> moduleFactory)
        {
            this.cacheFileSystem = cacheFileSystem;
            this.moduleFactory = moduleFactory;
        }

        readonly IFileSystem cacheFileSystem;
        readonly IModuleFactory<T> moduleFactory;
        readonly string containerFilename = "container.xml";
        readonly string versionFilename = "version";

        public bool IsUpToDate(DateTime dateTime, string version, IFileSystem sourceFileSystem)
        {
            if (!cacheFileSystem.FileExists(containerFilename)) return false;
            if (!cacheFileSystem.FileExists(versionFilename)) return false;
            
            if (IsSameVersion(version) == false) return false;

            var lastWriteTime = cacheFileSystem.GetLastWriteTimeUtc(containerFilename);
            if (lastWriteTime < dateTime) return false;
            
            var containerElement = LoadContainerElement(cacheFileSystem);
            if (AnyFilesMissing(containerElement, sourceFileSystem)) return false;
            if (AnyRawFileReferencesChanged(dateTime, containerElement, sourceFileSystem)) return false;

            return true;
        }

        bool IsSameVersion(string version)
        {
            using (var reader = new StreamReader(cacheFileSystem.OpenFile(versionFilename, FileMode.Open, FileAccess.Read)))
            {
                return reader.ReadLine() == version;
            }
        }

        bool AnyFilesMissing(XElement containerElement, IFileSystem sourceFileSystem)
        {
            return (from module in containerElement.Elements("module")
                    from asset in module.Elements("asset")
                    let path = Path.Combine(
                        module.Attribute("directory").Value,
                        asset.Attribute("filename").Value
                        )
                    select sourceFileSystem.FileExists(path)).Any(exists => !exists
            );
        }

        bool AnyRawFileReferencesChanged(DateTime lastWriteTime, XElement containerElement, IFileSystem sourceFileSystem)
        {
            return (from module in containerElement.Elements("module")
                    from reference in module.Elements("rawFileReference")
                    let filename = reference.Attribute("filename").Value
                    select sourceFileSystem.GetLastWriteTimeUtc(filename) > lastWriteTime).Any(changed => changed
            );
        }

        public IEnumerable<T> LoadModules()
        {
            var containerElement = LoadContainerElement(cacheFileSystem);
            var moduleElements = containerElement.Elements("module");
            var modules = CreateModules(moduleElements, cacheFileSystem);

            return modules;
        }

        XElement LoadContainerElement(IFileSystem fileSystem)
        {
            using (var containerFile = fileSystem.OpenFile(containerFilename, FileMode.Open, FileAccess.Read))
            {
                return XDocument.Load(containerFile).Root;
            }
        }

        IEnumerable<T> CreateModules(IEnumerable<XElement> moduleElements, IFileSystem fileSystem)
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
            var assetInfos = CreateAssetInfos(moduleElement, module.Path);
            var hash = ByteArrayExtensions.FromHexString(moduleElement.Attribute("hash").Value);
            var asset = new CachedAsset(hash, assetInfos, () => fileSystem.OpenFile(moduleFilename, FileMode.Open, FileAccess.Read));
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

        public void SaveModuleContainer(IModuleContainer<T> moduleContainer, string version)
        {
            cacheFileSystem.DeleteAll();
            SaveContainerXml(moduleContainer);
            SaveVersion(version);
            foreach (var module in moduleContainer.Modules.Where(m => m.IsPersistent))
            {
                SaveModule(module);
            }
        }

        void SaveContainerXml(IModuleContainer<T> moduleContainer)
        {
            var createManifestVisitor = new CreateManifestVisitor(m => GetModuleReferences(m, moduleContainer));
            var xml = new XDocument(
                new XElement("container",
                    from module in moduleContainer.Modules
                    where module.IsPersistent
                    select createManifestVisitor.CreateManifest(module)
                )
            );
            using (var fileStream = cacheFileSystem.OpenFile(containerFilename, FileMode.Create, FileAccess.Write))
            {
                xml.Save(fileStream);
            }
        }

        void SaveVersion(string version)
        {
            using (var writer = new StreamWriter(cacheFileSystem.OpenFile(versionFilename, FileMode.Create, FileAccess.Write)))
            {
                writer.Write(version);
            }
        }

        IEnumerable<string> GetModuleReferences(Module module, IModuleContainer<T> moduleContainer)
        {
            return (
                from asset in module.Assets
                from reference in asset.References
                where reference.Type == AssetReferenceType.DifferentModule
                let referencedModule = moduleContainer.FindModuleContainingPath(reference.ReferencedPath)
                select referencedModule.Path
            ).Distinct(StringComparer.OrdinalIgnoreCase);
        }

        void SaveModule(T module)
        {
            if (module.Assets.Count > 1)
            {
                throw new InvalidOperationException("Cannot save a module when assets have not been concatenated into a single asset.");
            }
            var filename = FlattenPathToSingleFilename(module.Path) + ".module";
            using (var fileStream = cacheFileSystem.OpenFile(filename, FileMode.Create, FileAccess.Write))
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
