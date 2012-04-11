using System.Collections.Generic;
using System.Linq;
using Cassette.Configuration;
using Cassette.Manifests;

namespace Cassette
{
    class CacheAwareBundleCollectionInitializer
    {
        readonly IEnumerable<IBundleDefinition> bundleDefinitions;
        readonly ICassetteManifestCache manifestCache;
        readonly IUrlModifier urlModifier;
        readonly CassetteSettings settings;
        readonly ExternalBundleGenerator externalBundleGenerator;
        BundleCollection bundles;

        public CacheAwareBundleCollectionInitializer(IEnumerable<IBundleDefinition> bundleDefinitions, ICassetteManifestCache manifestCache, IUrlModifier urlModifier, CassetteSettings settings, ExternalBundleGenerator externalBundleGenerator)
        {
            this.bundleDefinitions = bundleDefinitions;
            this.manifestCache = manifestCache;
            this.urlModifier = urlModifier;
            this.settings = settings;
            this.externalBundleGenerator = externalBundleGenerator;
        }

        public void Initialize(BundleCollection bundleCollection)
        {
            bundles = bundleCollection;
            using (bundles.GetWriteLock())
            {
                AddBundlesFromDefinitions();

                var cachedManifest = manifestCache.LoadCassetteManifest();
                var currentManifest = CreateCurrentManifest();
                if (CanUseCachedBundles(cachedManifest, currentManifest))
                {
                    ReplaceBundlesWithCachedBundles(cachedManifest);
                }
                else
                {
                    UseCurrentBundles();
                }
                bundles.BuildReferences();
            }
        }

        void AddBundlesFromDefinitions()
        {
            bundles.AddRange(bundleDefinitions);
        }

        void UseCurrentBundles()
        {
            ProcessBundles();
            AddBundlesForUrlReferences();
            SaveManifestToCache();
        }

        void AddBundlesForUrlReferences()
        {
            externalBundleGenerator.AddBundlesForUrlReferences(bundles);
        }

        void SaveManifestToCache()
        {
            var recreatedCurrentManifest = CreateCurrentManifest();
            manifestCache.SaveCassetteManifest(recreatedCurrentManifest);
            ReplaceBundlesWithCachedBundles(recreatedCurrentManifest);
        }

        void ReplaceBundlesWithCachedBundles(CassetteManifest cachedManifest)
        {
            bundles.Clear();
            var cachedBundles = cachedManifest.CreateBundles(urlModifier);
            bundles.AddRange(cachedBundles);
        }

        CassetteManifest CreateCurrentManifest()
        {
            return new CassetteManifest(settings.Version, bundles.Select(bundle => bundle.CreateBundleManifest()));
        }

        bool CanUseCachedBundles(CassetteManifest cachedManifest, CassetteManifest currentManifest)
        {
            return cachedManifest.Equals(currentManifest) &&
                   cachedManifest.IsUpToDateWithFileSystem(settings.SourceDirectory);
        }

        void ProcessBundles()
        {
            bundles.Process();
        }
    }
}