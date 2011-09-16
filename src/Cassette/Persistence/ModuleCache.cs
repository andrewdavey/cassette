using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.IO;
using Cassette.Utilities;
using System.Web;

namespace Cassette.Persistence
{
    public class ModuleCache<T> : IModuleCache<T>
        where T : Module
    {
        public ModuleCache(string version, IDirectory cacheDirectory, IDirectory sourceDirectory)
        {
            this.version = version;
            this.cacheDirectory = cacheDirectory;
            this.sourceDirectory = sourceDirectory;
            containerFile = cacheDirectory.GetFile(ContainerFilename);
        }

        const string ContainerFilename = "container.xml";

        readonly string version;
        readonly IDirectory cacheDirectory;
        readonly IDirectory sourceDirectory;
        readonly IFile containerFile;

        public bool InitializeModulesFromCacheIfUpToDate(IEnumerable<T> unprocessedSourceModules)
        {
            if (!containerFile.Exists) return false;

            var modules = unprocessedSourceModules.ToArray();
            if (CacheFileIsOlderThanAnyAsset(modules)) return false;

            var containerXml = LoadContainerElement();
            if (!IsSameVersion(containerXml)) return false;
            if (!IsSameAssetCount(modules, containerXml)) return false;
            if (!FilesAreUpToDate(containerXml)) return false;

            var moduleInitializationActions = (
                from module in modules
                select CreateModuleInitializationAction(module, containerXml)
            ).ToArray();

            if (moduleInitializationActions.Any(c => c == null)) return false;

            foreach (var assignCachedAsset in moduleInitializationActions)
            {
                assignCachedAsset();
            }

            return true;
        }

        bool FilesAreUpToDate(XElement containerXml)
        {
            var filePaths = (
                from e in containerXml.Descendants("File")
                let pathAttribute = e.Attribute("Path")
                where pathAttribute != null
                select pathAttribute.Value.Substring(2) // Removes the "~/" prefix
            );

            var files = filePaths
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(sourceDirectory.GetFile)
                .ToArray();

            if (files.Length == 0) return true;

            if (files.Any(f => !f.Exists)) return false;

            var maxLastWriteTimeUtc = files.Select(f => f.LastWriteTimeUtc).Max();
            return maxLastWriteTimeUtc <= containerFile.LastWriteTimeUtc;
        }

        bool CacheFileIsOlderThanAnyAsset(IEnumerable<T> unprocessedSourceModules)
        {
            var finder = new AssetLastWriteTimeFinder();
            finder.Visit(unprocessedSourceModules);
            return containerFile.LastWriteTimeUtc < finder.MaxLastWriteTimeUtc;
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

        Action CreateModuleInitializationAction(T module, XElement containerElement)
        {
            var moduleElement = GetModuleElement(module, containerElement);
            if (moduleElement == null) return null;

            var hash = GetHash(moduleElement);
            if (hash == null) return null;

            var filename = ModuleAssetCacheFilename(module);
            var file = cacheDirectory.GetFile(filename);
            if (module.Assets.Count > 0 && !file.Exists) return null;

            var references = GetModuleReferences(moduleElement).ToArray();

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

        IEnumerable<string> GetModuleReferences(XElement moduleElement)
        {
            return from e in moduleElement.Elements("Reference")
                   let pathAttribute = e.Attribute("Path")
                   where pathAttribute != null
                   select pathAttribute.Value;
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

        public void SaveModuleContainer(IModuleContainer<T> moduleContainer)
        {
            cacheDirectory.DeleteAll();
            SaveContainerXml(moduleContainer);
            foreach (var module in moduleContainer.Modules)
            {
                SaveModule(module);
            }
        }

        XElement LoadContainerElement()
        {
            using (var containerFileStream = containerFile.Open(FileMode.Open, FileAccess.Read))
            {
                return XDocument.Load(containerFileStream).Root;
            }
        }

        void SaveContainerXml(IModuleContainer<T> moduleContainer)
        {
            var assetCounter = new AssetCounter();
            assetCounter.Visit(moduleContainer.Modules);

            var xml = new XDocument(
                new XElement(
                    "Container",
                    new XAttribute("Version", version),
                    new XAttribute("AssetCount", assetCounter.Count),
                    from module in moduleContainer.Modules
                    select CreateModuleElement(module, moduleContainer)
                )
            );
            using (var fileStream = containerFile.Open(FileMode.Create, FileAccess.Write))
            {
                xml.Save(fileStream);
            }
        }

        XElement CreateModuleElement(T module, IModuleContainer<T> moduleContainer)
        {
            var referenceElements = CreateReferenceElements(module, moduleContainer);
            var fileElements = CreateFileElements(module);
            return new XElement(
                "Module",
                new XAttribute("Path", module.Path),
                referenceElements,
                fileElements
            );
        }

        IEnumerable<XElement> CreateFileElements(T module)
        {
            var fileReferences = (
                from asset in module.Assets
                from r in asset.References
                where r.Type == AssetReferenceType.RawFilename
                select r.Path
            ).Distinct(StringComparer.OrdinalIgnoreCase);

            return from file in fileReferences
                   select new XElement("File", new XAttribute("Path", file));
        }

        IEnumerable<XElement> CreateReferenceElements(T module, IModuleContainer<T> moduleContainer)
        {
            var references = (
                from asset in module.Assets
                from r in asset.References
                where r.Type == AssetReferenceType.DifferentModule || r.Type == AssetReferenceType.Url
                select moduleContainer.FindModuleContainingPath(r.Path).Path
            ).Concat(module.References)
             .Distinct(StringComparer.OrdinalIgnoreCase);

            return from reference in references
                   select new XElement("Reference", new XAttribute("Path", reference));
        }

        void SaveModule(T module)
        {
            if (module.Assets.Count == 0) return;

            if (module.Assets.Count > 1)
            {
                throw new InvalidOperationException("Cannot cache a module when assets have not been concatenated into a single asset.");
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

        string ModuleAssetCacheFilename(Module module)
        {
            if (module.Path.IsUrl())
            {
                return HttpUtility.UrlEncode(module.Path) + ".module";
            }
            else
            {
                return module.Path.Substring(2) // Remove the "~/" prefix
                             .Replace(Path.DirectorySeparatorChar, '`')
                             .Replace(Path.AltDirectorySeparatorChar, '`')
                           + ".module";
            }
        }
    }
}