using System;
using System.Collections.Generic;
using Cassette.Utilities;

namespace Cassette.Configuration
{
    abstract class BundleContainerFactoryBase : IBundleContainerFactory
    {
        readonly IDictionary<Type, IBundleFactory<Bundle>> bundleFactories;
        readonly CassetteSettings settings;

        protected BundleContainerFactoryBase(IDictionary<Type, IBundleFactory<Bundle>> bundleFactories, CassetteSettings settings)
        {
            this.bundleFactories = bundleFactories;
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

        protected IEnumerable<Bundle> CreateExternalBundlesUrlReferences(IEnumerable<Bundle> bundlesArray)
        {
            var referencesAlreadyCreated = new HashSet<string>(); // TODO: use case-insensitive string comparer?
            foreach (var bundle in bundlesArray)
            {
                foreach (var reference in bundle.References)
                {
                    if (reference.IsUrl() == false) continue;
                    if (referencesAlreadyCreated.Contains(reference)) continue;

                    var externalBundle = CreateExternalBundle(reference, bundle);
                    referencesAlreadyCreated.Add(externalBundle.Path);
                    yield return externalBundle;
                }
                foreach (var asset in bundle.Assets)
                {
                    foreach (var assetReference in asset.References)
                    {
                        if (assetReference.Type != AssetReferenceType.Url ||
                            referencesAlreadyCreated.Contains(assetReference.Path)) continue;

                        var externalBundle = CreateExternalBundle(assetReference.Path, bundle);
                        referencesAlreadyCreated.Add(externalBundle.Path);
                        yield return externalBundle;
                    }
                }
            }
        }

        Bundle CreateExternalBundle(string reference, Bundle referencer)
        {
            var bundleFactory = GetBundleFactory(referencer.GetType());
            var externalBundle = bundleFactory.CreateExternalBundle(reference);
            externalBundle.Process(settings);
            return externalBundle;
        }

        IBundleFactory<Bundle> GetBundleFactory(Type bundleType)
        {
            IBundleFactory<Bundle> factory;
            if (bundleFactories.TryGetValue(bundleType, out factory))
            {
                return factory;
            }
            throw new ArgumentException(string.Format("Cannot find bundle factory for {0}", bundleType.FullName));
        }
    }
}