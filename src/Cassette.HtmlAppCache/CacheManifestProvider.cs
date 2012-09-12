using System;
using System.Collections.Generic;

namespace Cassette.HtmlAppCache
{
    public class CacheManifestProvider : ICacheManifestProvider
    {
        readonly Func<CacheManifestBuilder> createBuilder;
        CacheManifest cacheManifest;

        public CacheManifestProvider(BundleCollection bundles, Func<CacheManifestBuilder> createBuilder)
        {
            this.createBuilder = createBuilder;

            using (bundles.GetReadLock())
            {
                BuildCacheManifest(bundles);
            }

            bundles.Changed += BundlesOnChanged;
        }

        void BuildCacheManifest(IEnumerable<Bundle> bundles)
        {
            var builder = createBuilder();
            cacheManifest = builder.BuildCacheManifest(bundles);
        }

        void BundlesOnChanged(object sender, BundleCollectionChangedEventArgs args)
        {
            BuildCacheManifest(args.Bundles);
        }

        public CacheManifest GetCacheManifest()
        {
            return cacheManifest;
        }
    }
}