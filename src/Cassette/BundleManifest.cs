using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Utilities;

namespace Cassette
{
    internal abstract class BundleManifest
    {
        protected BundleManifest()
        {
            Assets = new List<AssetManifest>();
            References = new List<string>();
        }

        public string Path { get; set; }
        public byte[] Hash { get; set; }
        public string ContentType { get; set; }
        public string PageLocation { get; set; }
        public IList<AssetManifest> Assets { get; private set; }
        public IList<string> References { get; private set; }

        public override bool Equals(object obj)
        {
            var other = obj as BundleManifest;
            return other != null
                   && GetType() == other.GetType()
                   && Path.Equals(other.Path)
                   && AssetsEqual(other.Assets);
        }

        bool AssetsEqual(IEnumerable<AssetManifest> assets)
        {
            return Assets.SequenceEqual(assets);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public Bundle CreateBundle(IFile bundleContentFile)
        {
            var bundle = CreateBundleCore();
            bundle.Hash = Hash;
            bundle.ContentType = ContentType;
            bundle.PageLocation = PageLocation;
            bundle.IsFromCache = true;
            bundle.Assets.Add(CreateCachedBundleContent(bundleContentFile));
            bundle.Process(new CassetteSettings(""));
            return bundle;
        }

        protected abstract Bundle CreateBundleCore();

        CachedBundleContent CreateCachedBundleContent(IFile bundleContentFile)
        {
            return new CachedBundleContent(bundleContentFile, OriginalAssets());
        }

        IEnumerable<IAsset> OriginalAssets()
        {
            return Assets.Select(assetManifest => new AssetFromManifest(assetManifest.Path));
        }

        public virtual void InitializeFromXElement(XElement manifestElement)
        {
            Path = manifestElement.AttributeOrThrow("Path", () => new InvalidBundleManifestException("Bundle manifest element missing \"Path\" attribute."));
            Hash = GetHashFromElement(manifestElement);
            ContentType = manifestElement.AttributeValueOrNull("ContentType");
            PageLocation = manifestElement.AttributeValueOrNull("PageLocation");
            AddAssets(manifestElement);
            AddReferences(manifestElement);
        }

        byte[] GetHashFromElement(XElement manifestElement)
        {
            return ByteArrayExtensions.FromHexString(
                manifestElement.AttributeOrThrow("Hash", () => new InvalidBundleManifestException("Bundle manifest element missing \"Hash\" attribute."))
            );
        }

        void AddAssets(XElement manifestElement)
        {
            var assetElements = manifestElement.Elements("Asset");
            foreach (var assetElement in assetElements)
            {
                Assets.Add(DeserializeAssetManifest(assetElement));
            }
        }

        AssetManifest DeserializeAssetManifest(XElement assetElement)
        {
            var deserializer = new AssetManifestDeserializer();
            return deserializer.Deserialize(assetElement);
        }

        void AddReferences(XElement manifestElement)
        {
            var referenceElements = manifestElement.Elements("Reference");
            foreach (var referenceElement in referenceElements)
            {
                AddReference(referenceElement);
            }
        }

        void AddReference(XElement referenceElement)
        {
            var path = referenceElement.AttributeOrThrow("Path", () => new InvalidBundleManifestException("Reference manifest element missing \"Path\" attribute."));
            References.Add(path);
        }

        public XElement SerializeToXElement()
        {
            return new XElement(
                ConventionalXElementName,
                new XAttribute("Path", Path),
                new XAttribute("Hash", Hash.ToHexString()),
                new XAttribute("ContentType", ContentType),
                new XAttribute("PageLocation", PageLocation),
                Assets.Select(a => a.SerializeToXElement()),
                References.Select(SerializeReference)
            );
        }

        string ConventionalXElementName
        {
            get
            {
                var name = GetType().Name;
                return name.Substring(0, name.Length - "Manifest".Length);
            }
        }

        XElement SerializeReference(string path)
        {
            return new XElement(
                "Reference",
                new XAttribute("Path", path)
            );
        }
    }
}