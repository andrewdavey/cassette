﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.Manifests;

namespace Cassette.Configuration
{
    class CachedBundleContainerFactory : BundleContainerFactoryBase
    {
        readonly ICassetteManifestCache cache;
        readonly CassetteSettings settings;
        Bundle[] bundlesArray;

        public CachedBundleContainerFactory(ICassetteManifestCache cache, CassetteSettings settings)
            : base(settings)
        {
            this.cache = cache;
            this.settings = settings;
        }

        public override IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles)
        {
            bundlesArray = unprocessedBundles.ToArray();
            
            var currentManifest = CreateCassetteManifest();
            var cachedManifest = cache.LoadCassetteManifest();

            bundlesArray = CreateBundles(cachedManifest, currentManifest);

            var externalBundles = CreateExternalBundlesUrlReferences(bundlesArray);
            var allBundles = bundlesArray.Concat(externalBundles);
            return new BundleContainer(allBundles);
        }

        Bundle[] CreateBundles(CassetteManifest cachedManifest, CassetteManifest currentManifest)
        {
            var canUseCachedBundles = cachedManifest.Equals(currentManifest)
                                   && cachedManifest.IsUpToDateWithFileSystem(settings.SourceDirectory);
            if (canUseCachedBundles)
            {
                Trace.Source.TraceInformation("Using cache.");
                UseCachedBundles(cachedManifest);
            }
            else
            {
                CacheAndUseCurrentBundles();
            }
            return bundlesArray;
        }

        void UseCachedBundles(CassetteManifest cachedManifest)
        {
            bundlesArray = cachedManifest.CreateBundles().ToArray();
        }

        void CacheAndUseCurrentBundles()
        {
            //TODO: It will be nice for the bundle processors to be able register things like this to an dispose event so we don't have to have this logic here
            foreach (var sprite in settings.SpriteDirectory.GetFiles("*.png", SearchOption.TopDirectoryOnly))
            {
                sprite.Delete();
            }

            ProcessAllBundles(bundlesArray);
            var manifestIncludingContent = CreateCassetteManifest();
            Trace.Source.TraceInformation("Saving cache.");
            cache.SaveCassetteManifest(manifestIncludingContent);
            UseCachedBundles(manifestIncludingContent);
        }

        CassetteManifest CreateCassetteManifest()
        {
            var bundleManifests = bundlesArray.Select(bundle => bundle.CreateBundleManifest());
            return new CassetteManifest(settings.Version, bundleManifests);
        }
    }
}