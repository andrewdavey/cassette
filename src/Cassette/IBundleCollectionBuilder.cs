using System.Collections.Generic;
using System.Linq;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Manifests;
namespace Cassette
{
    public interface IBundleCollectionBuilder
    {
        void BuildBundleCollection(BundleCollection bundles);
    }

    public class BundleCollectionBuilder : IBundleCollectionBuilder
    {
        readonly CassetteSettings settings;
        readonly IUrlModifier urlModifier;

        public BundleCollectionBuilder(CassetteSettings settings, IUrlModifier urlModifier)
        {
            this.settings = settings;
            this.urlModifier = urlModifier;
        }

        public void BuildBundleCollection(BundleCollection bundles)
        {
            IBundleCollectionBuilder builder;
            if (settings.PrecompiledManifestFile.Exists)
            {
                builder = new PrecompiledBundleCollectionBuilder(settings.PrecompiledManifestFile, urlModifier);
            }
            else if (settings.IsDebuggingEnabled)
            {
                builder = new DebugModeBundleCollectionBuilder();
            }
            else
            {
                builder = new ProductionModeBundleCollectionBuilder();
            }

            builder.BuildBundleCollection(bundles);
        }
    }

    class ProductionModeBundleCollectionBuilder : IBundleCollectionBuilder
    {
        readonly IEnumerable<IBundleDefinition> bundleDefinitions;
        readonly ICassetteManifestCache manifestCache;
        readonly IUrlModifier urlModifier;
        readonly CassetteSettings settings;
        readonly ExternalBundleGenerator externalBundleGenerator;
        BundleCollection bundles;

        public ProductionModeBundleCollectionBuilder(IEnumerable<IBundleDefinition> bundleDefinitions, ICassetteManifestCache manifestCache, IUrlModifier urlModifier, CassetteSettings settings, ExternalBundleGenerator externalBundleGenerator)
        {
            this.bundleDefinitions = bundleDefinitions;
            this.manifestCache = manifestCache;
            this.urlModifier = urlModifier;
            this.settings = settings;
            this.externalBundleGenerator = externalBundleGenerator;
        }

        public void BuildBundleCollection(BundleCollection bundleCollection)
        {
            bundles = bundleCollection;

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
        }

        void AddBundlesFromDefinitions()
        {
            foreach (var bundleDefinition in bundleDefinitions)
            {
                bundleDefinition.AddBundles(bundles);
            }
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
            foreach (var cachedBundle in cachedBundles)
            {
                bundles.Add(cachedBundle);
            }
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
            foreach (var bundle in bundles)
            {
                bundle.Process(settings);
            }
        }
    }

    class DebugModeBundleCollectionBuilder : IBundleCollectionBuilder
    {
        readonly IEnumerable<IBundleDefinition> bundleDefinitions;
        readonly CassetteSettings settings;
        readonly ExternalBundleGenerator externalBundleGenerator;

        public DebugModeBundleCollectionBuilder(IEnumerable<IBundleDefinition> bundleDefinitions, CassetteSettings settings, ExternalBundleGenerator externalBundleGenerator)
        {
            this.bundleDefinitions = bundleDefinitions;
            this.settings = settings;
            this.externalBundleGenerator = externalBundleGenerator;
        }

        public void BuildBundleCollection(BundleCollection bundles)
        {
            AddBundlesFromEachDefinition(bundles);
            ProcessBundles(bundles);
            externalBundleGenerator.AddBundlesForUrlReferences(bundles);
        }

        void AddBundlesFromEachDefinition(BundleCollection bundles)
        {
            foreach (var bundleDefinition in bundleDefinitions)
            {
                bundleDefinition.AddBundles(bundles);
            }
        }

        void ProcessBundles(IEnumerable<Bundle> bundles)
        {
            foreach (var bundle in bundles)
            {
                bundle.Process(settings);
            }
        }
    }

    class PrecompiledBundleCollectionBuilder : IBundleCollectionBuilder
    {
        readonly IFile precompiledManifestFile;
        readonly IUrlModifier urlModifier;

        public PrecompiledBundleCollectionBuilder(IFile precompiledManifestFile, IUrlModifier urlModifier)
        {
            this.precompiledManifestFile = precompiledManifestFile;
            this.urlModifier = urlModifier;
        }

        public void BuildBundleCollection(BundleCollection bundles)
        {
            var manifest = ReadManifest();
            var createdBundles = manifest.CreateBundles(urlModifier);
            foreach (var bundle in createdBundles)
            {
                bundles.Add(bundle);
            }
        }

        CassetteManifest ReadManifest()
        {
            using (var stream = precompiledManifestFile.OpenRead())
            {
                var reader = new CassetteManifestReader(stream);
                return reader.Read();
            }
        }
    }
}