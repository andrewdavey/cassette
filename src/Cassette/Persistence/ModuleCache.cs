using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Persistence
{
    public class ModuleCache<T> : IModuleCache<T>
        where T : Module
    {
        public ModuleCache(string version, IDirectory cacheDirectory)
        {
            this.version = version;
            this.cacheDirectory = cacheDirectory;
            containerFile = cacheDirectory.GetFile(ContainerFilename);
        }

        const string ContainerFilename = "container.xml";

        readonly string version;
        readonly IDirectory cacheDirectory;
        readonly IFile containerFile;

        public bool LoadContainerIfUpToDate(IEnumerable<T> unprocessedSourceModules, out IModuleContainer<T> container)
        {
            container = null;

            if (!containerFile.Exists) return false;

            if (!CacheFileIsOlderThanAssets(unprocessedSourceModules)) return false;

            var containerXml = LoadContainerElement();

            if (!IsSameVersion(containerXml)) return false;

            if (!IsSameAssetCount(unprocessedSourceModules, containerXml)) return false;

            var assignCachedAssets = (
                from module in unprocessedSourceModules
                select CreateCachedAsset(module, containerXml)
            ).ToArray();

            if (assignCachedAssets.Any(c => c == null)) return false;

            foreach (var assignCachedAsset in assignCachedAssets)
            {
                assignCachedAsset();
            }

            container = new ModuleContainer<T>(unprocessedSourceModules);
            return true;
        }

        bool CacheFileIsOlderThanAssets(IEnumerable<T> unprocessedSourceModules)
        {
            var finder = new AssetLastWriteTimeFinder();
            finder.Visit(unprocessedSourceModules);
            return containerFile.LastWriteTimeUtc >= finder.MaxLastWriteTimeUtc;
        }

        bool IsSameVersion(XElement containerXml)
        {
            var versionAttribute = containerXml.Attribute("Version");
            if (versionAttribute == null) return false;

            return versionAttribute.Value == version;
        }

        bool IsSameAssetCount(IEnumerable<T> unprocessedSourceModules, XElement containerXml)
        {
            var assetCountAttribute = containerXml.Attribute("AssetCount");
            if (assetCountAttribute == null) return false;
            
            int assetCount;
            if (int.TryParse(assetCountAttribute.Value, out assetCount) == false) return false;
            
            var assetCounter = new AssetCounter();
            assetCounter.Visit(unprocessedSourceModules);

            return assetCount == assetCounter.Count;
        }

        Action CreateCachedAsset(T module, XElement containerElement)
        {
            var moduleElement = GetModuleElement(module, containerElement);
            if (moduleElement == null) return null;

            var hash = GetHash(moduleElement);
            if (hash == null) return null;

            var filename = ModuleAssetCacheFilename(module);
            var file = cacheDirectory.GetFile(filename);
            if (module.Assets.Count > 0 && !file.Exists) return null;

            var references = GetModuleReferences(moduleElement);

            var childAssets = module.Assets.ToArray();
            return () =>
            {
                if (module.Assets.Count > 0)
                {
                    module.Assets.Clear();
                    module.Assets.Add(new CachedAsset(file, hash, childAssets));
                }
                module.AddReferences(references);
            };
        }

        string[] GetModuleReferences(XElement moduleElement)
        {
            return (
                from e in moduleElement.Elements("Reference")
                let pathAttribute = e.Attribute("Path")
                where pathAttribute != null
                select pathAttribute.Value
            ).ToArray();
        }

        XElement GetModuleElement(T module, XElement containerElement)
        {
            return (
                from e in containerElement.Elements("Module")
                let pathAttribute = e.Attribute("Path")
                where pathAttribute != null
                   && pathAttribute.Value == module.Path
                select e
            ).FirstOrDefault();
        }

        byte[] GetHash(XElement moduleElement)
        {
            var attribute = moduleElement.Attribute("Hash");
            if (attribute == null) return null;
            return ByteArrayExtensions.FromHexString(attribute.Value);
        }

        public IModuleContainer<T> SaveModuleContainer(IEnumerable<T> modules)
        {
            cacheDirectory.DeleteAll();
            SaveContainerXml(modules);
            foreach (var module in modules)
            {
                SaveModule(module);
            }
            return new CachedModuleContainer<T>(modules);
        }

        XElement LoadContainerElement()
        {
            using (var containerFileStream = containerFile.Open(FileMode.Open, FileAccess.Read))
            {
                return XDocument.Load(containerFileStream).Root;
            }
        }

        void SaveContainerXml(IEnumerable<T> modules)
        {
            var xml = new XDocument(
                new XElement("container",
                    new XAttribute("version", version),
                    modules.SelectMany(module => module.CreateCacheManifest())
                )
            );
            using (var fileStream = containerFile.Open(FileMode.Create, FileAccess.Write))
            {
                xml.Save(fileStream);
            }
        }

        void SaveModule(T module)
        {
            if (module.Assets.Count > 1)
            {
                throw new InvalidOperationException("Cannot cache a module when assets have not been concatenated into a single asset.");
            }
            if (module.Assets.Count == 0)
            {
                return; // Skip external URL modules.
            }

            var file = cacheDirectory.GetFile(ModuleAssetCacheFilename(module));
            using (var fileStream = file.Open(FileMode.Create, FileAccess.Write))
            {
                using (var dataStream = module.Assets[0].OpenStream())
                {
                    dataStream.CopyTo(fileStream);
                }
                fileStream.Flush();
            }
        }

        internal static string ModuleAssetCacheFilename(Module module)
        {
            return module.Path.Substring(2) // Remove the "~/" prefix
                .Replace(Path.DirectorySeparatorChar, '`')
                .Replace(Path.AltDirectorySeparatorChar, '`')
                + ".module";
        }
    }

    class AssetLastWriteTimeFinder : IAssetVisitor
    {
        DateTime max;

        public DateTime MaxLastWriteTimeUtc
        {
            get { return max; }
        }

        public void Visit(Module module)
        {
        }

        public void Visit(IAsset asset)
        {
            var lastWriteTimeUtc = asset.SourceFile.LastWriteTimeUtc;
            if (lastWriteTimeUtc > MaxLastWriteTimeUtc)
            {
                max = lastWriteTimeUtc;
            }
        }

        public void Visit(IEnumerable<Module> unprocessedSourceModules)
        {
            foreach (var module in unprocessedSourceModules)
            {
                module.Accept(this);
            }
        }
    }

    class AssetCounter : IAssetVisitor
    {
        int count;

        public int Count
        {
            get { return count; }
        }

        public void Visit(Module module)
        {
        }

        public void Visit(IAsset asset)
        {
            count = Count + 1;
        }

        public void Visit(IEnumerable<Module> unprocessedSourceModules)
        {
            foreach (var module in unprocessedSourceModules)
            {
                module.Accept(this);
            }
        }
    }
}
