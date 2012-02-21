using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.IO;

namespace Cassette.Manifests
{
    abstract class BundleManifest
    {
        protected BundleManifest()
        {
            Assets = new List<AssetManifest>();
            References = new List<string>();
            HtmlAttributes = new Dictionary<string, string>();
        }

        public string Path { get; set; }
        public byte[] Hash { get; set; }
        public string ContentType { get; set; }
        public string PageLocation { get; set; }
        public IList<AssetManifest> Assets { get; private set; }
        public IList<string> References { get; private set; }
        public IDictionary<string, string> HtmlAttributes { get; private set; }
        public byte[] Content { get; set; }

        public Bundle CreateBundle()
        {
            var bundle = CreateBundleCore();
            bundle.Hash = Hash;
            bundle.ContentType = ContentType;
            bundle.PageLocation = PageLocation;
            bundle.IsFromCache = true;
            if (Assets.Count > 0)
            {
                bundle.Assets.Add(CreateCachedBundleContent());
            }
            AddReferencesToBundle(bundle);
            AddHtmlAttributesToBundle(bundle);
            return bundle;
        }

        void AddHtmlAttributesToBundle(Bundle bundle)
        {
            foreach (var htmlAttribute in HtmlAttributes)
            {
                bundle.HtmlAttributes.Add(htmlAttribute.Key, htmlAttribute.Value);
            }
        }

        protected abstract Bundle CreateBundleCore();

        CachedBundleContent CreateCachedBundleContent()
        {
            return new CachedBundleContent(Content, CreateOriginalAssets());
        }

        IEnumerable<IAsset> CreateOriginalAssets()
        {
            return Assets.Select(assetManifest => new AssetFromManifest(assetManifest)).Cast<IAsset>();
        }

        void AddReferencesToBundle(Bundle bundle)
        {
            foreach (var reference in References)
            {
                bundle.AddReference(reference);
            }
        }

        public bool IsUpToDateWithFileSystem(IDirectory directory, DateTime asOfDateTime)
        {
            return Assets.All(assetManifest => assetManifest.IsUpToDateWithFileSystem(directory, asOfDateTime));
        }

        public override bool Equals(object obj)
        {
            var other = obj as BundleManifest;
            return other != null
                   && GetType() == other.GetType()
                   && Path.Equals(other.Path, StringComparison.OrdinalIgnoreCase)
                   && AssetsEqual(other.Assets);
        }

        bool AssetsEqual(IEnumerable<AssetManifest> assets)
        {
            return Assets.OrderBy(a => a.Path).SequenceEqual(assets.OrderBy(a => a.Path));
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }
    }
}