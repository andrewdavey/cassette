using System.Collections.Generic;
using System.Linq;
using Cassette.IO;
using System;

namespace Cassette.Manifests
{
    abstract class BundleManifest
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
        public byte[] Content { get; set; }

        public Bundle CreateBundle()
        {
            var bundle = CreateBundleCore();
            bundle.Hash = Hash;
            bundle.ContentType = ContentType;
            bundle.PageLocation = PageLocation;
            bundle.IsFromCache = true;
            bundle.Assets.Add(CreateCachedBundleContent());
            return bundle;
        }

        protected abstract Bundle CreateBundleCore();

        CachedBundleContent CreateCachedBundleContent()
        {
            return new CachedBundleContent(Content, CreateOriginalAssets());
        }

        IEnumerable<IAsset> CreateOriginalAssets()
        {
            return Assets.Select(assetManifest => new AssetFromManifest(assetManifest.Path));
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
    }
}