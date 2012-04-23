using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;

namespace Cassette
{
    /// <summary>
    /// Modifies a <see cref="BundleCollection"/> by adding external bundles for any URL references made by existing bundles,
    /// which are not already represented by external bundles.
    /// </summary>
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
            existingUrls = GetExistingUrls(bundleCollection);

            bundleCollection.Accept(this);

            bundleCollection.AddRange(createdExternalBundles);
        }

        HashSet<string> GetExistingUrls(IEnumerable<Bundle> bundleCollection)
        {
            return new HashSet<string>(
                bundleCollection
                    .OfType<IExternalBundle>()
                    .Select(b => b.ExternalUrl)
            );
        }

        void IBundleVisitor.Visit(Bundle bundle)
        {
            currentBundle = bundle;

            var urlReferencesNotYetSeen = bundle.References.Where(IsUrlReferenceNotYetSeen);
            foreach (var reference in urlReferencesNotYetSeen)
            {
                RecordUrlAsSeen(reference);
                AddNewExternalBundle(reference);
            }
        }

        bool IsUrlReferenceNotYetSeen(string reference)
        {
            var alreadySeenUrl = existingUrls.Contains(reference);
            return reference.IsUrl() && !alreadySeenUrl;
        }

        void IBundleVisitor.Visit(IAsset asset)
        {
            foreach (var assetReference in asset.References.Where(IsUrlReferenceNotYetSeen))
            {
                RecordUrlAsSeen(assetReference.Path);
                AddNewExternalBundle(assetReference.Path);
            }
        }

        void RecordUrlAsSeen(string url)
        {
            existingUrls.Add(url);
        }

        bool IsUrlReferenceNotYetSeen(AssetReference reference)
        {
            var isUrl = reference.Type == AssetReferenceType.Url;
            var alreadySeenUrl = existingUrls.Contains(reference.Path);
            return isUrl && !alreadySeenUrl;
        }

        void AddNewExternalBundle(string url)
        {
            createdExternalBundles.Add(CreateExternalBundle(url, currentBundle));
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