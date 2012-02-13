using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;
#if NET35
using Iesi.Collections.Generic;
#endif

namespace Cassette.Configuration
{
    abstract class BundleContainerFactoryBase : IBundleContainerFactory
    {
        readonly CassetteSettings settings;

        protected BundleContainerFactoryBase(CassetteSettings settings)
        {
            this.settings = settings;
        }

        public abstract IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles);

        protected void ProcessAllBundles(IEnumerable<Bundle> bundles)
        {
            Trace.Source.TraceInformation("Processing bundles...");
            foreach (var bundle in bundles)
            {
                Trace.Source.TraceInformation("Processing {0} {1}", bundle.GetType().Name, bundle.Path);
                bundle.Process(settings);
            }
            Trace.Source.TraceInformation("Bundle processing completed.");
        }

        protected IEnumerable<Bundle> CreateExternalBundlesUrlReferences(Bundle[] bundlesArray)
        {
            var existingUrls = bundlesArray.OfType<IExternalBundle>().Select(b => b.Url);
            var generator = new ExternalBundleGenerator(existingUrls, settings);
            foreach (var bundle in bundlesArray)
            {
                bundle.Accept(generator);
            }
            return generator.ExternalBundles;
        }
    }

    class ExternalBundleGenerator : IBundleVisitor
    {
        readonly CassetteSettings settings;
        readonly HashedSet<string> existingUrls;
        readonly List<Bundle> bundles = new List<Bundle>();
        Bundle currentBundle;

        public ExternalBundleGenerator(IEnumerable<string> existingUrls, CassetteSettings settings)
        {
            this.settings = settings;
            this.existingUrls = new HashedSet<string>(new List<string>(existingUrls)); // TODO: use case-insensitive string comparer?
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
            IBundleFactory<Bundle> factory;
            if (settings.BundleFactories.TryGetValue(bundleType, out factory))
            {
                return factory;
            }
            throw new ArgumentException(string.Format("Cannot find bundle factory for {0}", bundleType.FullName));
        }
    }
}