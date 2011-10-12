using System;
using System.Collections.Generic;
using Cassette.Utilities;

namespace Cassette.Configuration
{
    public abstract class BundleContainerFactoryBase
    {
        protected BundleContainerFactoryBase(ICassetteApplication application, IDictionary<Type, IBundleFactory<Bundle>> bundleFactories)
        {
            this.application = application;
            this.bundleFactories = bundleFactories;
        }

        readonly ICassetteApplication application;
        readonly IDictionary<Type, IBundleFactory<Bundle>> bundleFactories;

        public abstract IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles);

        protected void ProcessAllBundles(IEnumerable<Bundle> bundlesArray)
        {
            foreach (var bundle in bundlesArray)
            {
                bundle.Process(application);
            }
        }

        protected IEnumerable<Bundle> CreateExternalBundlesFromReferences(IEnumerable<Bundle> bundlesArray)
        {
            var referencesAlreadyCreated = new HashSet<string>();
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
            }
        }

        Bundle CreateExternalBundle(string reference, Bundle referencer)
        {
            var bundleFactory = GetBundleFactory(referencer.GetType());
            return bundleFactory.CreateExternalBundle(reference);
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