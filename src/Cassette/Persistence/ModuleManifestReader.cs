using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Persistence
{
    /// <summary>
    /// Converts an XML module cache manifest into module and asset objects.
    /// </summary>
    public class ModuleManifestReader<T>
        where T : Module
    {
        readonly IDirectory rootDirectory;
        readonly IModuleFactory<T> moduleFactory;

        public ModuleManifestReader(IDirectory rootDirectory, IModuleFactory<T> moduleFactory)
        {
            this.rootDirectory = rootDirectory;
            this.moduleFactory = moduleFactory;
        }

        public IEnumerable<T> CreateModules(XElement rootElement)
        {
            var moduleElements = rootElement.Elements("Module");
            return moduleElements.Select(CreateModule);
        }

        protected virtual T CreateModule(XElement moduleElement)
        {
            var pathAttribute = moduleElement.Attribute("Path");
            var hashAttribute = moduleElement.Attribute("Hash");
            var assetElements = moduleElement.Elements("Asset");
            var referenceElements = moduleElement.Elements("Reference");
            if (pathAttribute == null || hashAttribute == null) throw new ArgumentException("Invalid module manifest data.");

            var module = moduleFactory.CreateModule(pathAttribute.Value);
            AssignModuleContentType(module, moduleElement);
            AddCachedAssetToModule(assetElements, hashAttribute, module);
            AddReferencesToModule(referenceElements, module);
            module.InitializeFromManifest(moduleElement);
            return module;
        }

        void AddReferencesToModule(IEnumerable<XElement> referenceElements, T module)
        {
            var references = new List<string>();
            foreach (var element in referenceElements)
            {
                var pathAttribute = element.Attribute("Path");
                if (pathAttribute == null) throw new ArgumentException("Invalid module manifest data.");
                references.Add(pathAttribute.Value);
            }
            module.AddReferences(references);
        }

        void AssignModuleContentType(Module module, XElement moduleElement)
        {
            var contentTypeAttribute = moduleElement.Attribute("ContentType");
            if (contentTypeAttribute != null) module.ContentType = contentTypeAttribute.Value;
        }

        void AddCachedAssetToModule(IEnumerable<XElement> assetElements, XAttribute hashAttribute, T module)
        {
            var filename = ModuleCache<T>.ModuleAssetCacheFilename(module);
            var hash = ByteArrayExtensions.FromHexString(hashAttribute.Value);
            var children = assetElements.Select(CreateCachedAssetSourceInfo);

            var cachedAsset = new CachedAsset(
                rootDirectory.GetFile(filename),
                hash,
                children
            );
            module.Assets.Add(cachedAsset);
        }

        IAsset CreateCachedAssetSourceInfo(XElement element)
        {
            var path = element.Attribute("Path");
            var references = element.Elements("Reference");

            if (path == null)
            {
                throw new ArgumentException("Invalid asset manifest data. The \"Path\" attribute is missing from the Asset element.");
            }

            var asset = new CachedAssetSourceInfo(path.Value);
            asset.AddReferences(references.Select(r => CreateReference(r, asset)));
            return asset;
        }

        AssetReference CreateReference(XElement referenceElement, IAsset asset)
        {
            var typeAttribute = referenceElement.Attribute("Type");
            var pathAttribute = referenceElement.Attribute("Path");
            var sourceLineNumberAttribute = referenceElement.Attribute("SourceLineNumber");

            if (typeAttribute == null)
            {
                throw new ArgumentException(
                    "Invalid reference manifest data. The \"Type\" attribute is missing from the Reference element."
                );
            }
            if (pathAttribute == null)
            {
                throw new ArgumentException(
                    "Invalid reference manifest data. The \"Path\" attribute is missing from the Reference element."
                );
            }
            if (sourceLineNumberAttribute == null)
            {
                throw new ArgumentException(
                    "Invalid reference manifest data. The \"SourceLineNumber\" attribute is missing from the Reference element."
                );
            }

            int sourceLineNumber;
            AssetReferenceType type;
            if (int.TryParse(sourceLineNumberAttribute.Value, out sourceLineNumber) == false)
            {
                throw new ArgumentException(
                    "Invalid reference manifest data. The \"SourceLineNumber\" attribute is not an integer."
                );
            }

            if (Enum.TryParse(typeAttribute.Value, out type) == false)
            {
                throw new ArgumentException(
                    "Invalid reference manifest data. The \"Type\" is not a recognised Cassette.AssetReferenceType value."
                );
            }

            return new AssetReference(pathAttribute.Value, asset, int.Parse(sourceLineNumberAttribute.Value), type);
        }
    }
}
