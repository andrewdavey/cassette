using System;
using System.Collections.Generic;
using Cassette.Utilities;

namespace Cassette.Configuration
{
    class ExternalBundleGenerator : IBundleVisitor
    {
        readonly CassetteSettings settings;
        readonly HashSet<string> existingUrls;
        readonly List<Bundle> bundles = new List<Bundle>();
        Bundle currentBundle;

        public ExternalBundleGenerator(IEnumerable<string> existingUrls, CassetteSettings settings)
        {
            this.settings = settings;
            this.existingUrls = new HashSet<string>(existingUrls); // TODO: use case-insensitive string comparer?
        }

        public IEnumerable<Bundle> ExternalBundles
        {
            get { return bundles; }
        }

        public void Visit(Bundle bundle)
        {
            currentBundle = bundle;

            foreach (var reference in bundle.References)
            {
                if (reference.IsUrl() == false) continue;
                if (existingUrls.Contains(reference)) continue;

                existingUrls.Add(reference);
                bundles.Add(CreateExternalBundle(reference, currentBundle));
            }
        }

        public void Visit(IAsset asset)
        {
            foreach (var assetReference in asset.References)
            {
                if (assetReference.Type != AssetReferenceType.Url) continue;
                if (existingUrls.Contains(assetReference.Path)) continue;

                existingUrls.Add(assetReference.Path);
                bundles.Add(CreateExternalBundle(assetReference.Path, currentBundle));
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
            return settings.GetDefaults(bundleType).BundleFactory;
        }
    }
}