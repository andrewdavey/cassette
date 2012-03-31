using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;

namespace Cassette.Configuration
{
    class ExternalBundleGenerator : IBundleVisitor
    {
        readonly IBundleFactoryProvider bundleFactoryProvider;
        readonly CassetteSettings settings;
        HashSet<string> existingUrls;
        List<Bundle> createdExternalBundles;
        Bundle currentBundle;

        public ExternalBundleGenerator(IBundleFactoryProvider bundleFactoryProvider, CassetteSettings settings)
        {
            this.bundleFactoryProvider = bundleFactoryProvider;
            this.settings = settings;
        }

        public void AddBundlesForUrlReferences(BundleCollection bundleCollection)
        {
            createdExternalBundles = new List<Bundle>();
            existingUrls = new HashSet<string>(bundleCollection.OfType<IExternalBundle>().Select(b => b.ExternalUrl)); // TODO: use case-insensitive string comparer?

            bundleCollection.Accept(this);

            foreach (var bundle in createdExternalBundles)
            {
                bundleCollection.Add(bundle);
            }
        }

        void IBundleVisitor.Visit(Bundle bundle)
        {
            currentBundle = bundle;

            foreach (var reference in bundle.References)
            {
                if (reference.IsUrl() == false) continue;
                if (existingUrls.Contains(reference)) continue;

                existingUrls.Add(reference);
                createdExternalBundles.Add(CreateExternalBundle(reference, currentBundle));
            }
        }

        void IBundleVisitor.Visit(IAsset asset)
        {
            foreach (var assetReference in asset.References)
            {
                if (assetReference.Type != AssetReferenceType.Url) continue;
                if (existingUrls.Contains(assetReference.Path)) continue;

                existingUrls.Add(assetReference.Path);
                createdExternalBundles.Add(CreateExternalBundle(assetReference.Path, currentBundle));
            }
        }

        Bundle CreateExternalBundle(string url, Bundle referencer)
        {
            var bundleFactory = GetBundleFactory(referencer.GetType());
            var externalBundle = bundleFactory.CreateExternalBundle(url);
            externalBundle.Process(settings);
            return externalBundle;
        }

        IBundleFactory<Bundle> GetBundleFactory(Type bundleType)
        {
            return bundleFactoryProvider.GetBundleFactory(bundleType);
        }
    }
}