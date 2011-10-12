using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.Configuration
{
    public class BundleContainerFactory : BundleContainerFactoryBase
    {
        public BundleContainerFactory(ICassetteApplication application, IDictionary<Type, IBundleFactory<Bundle>> factories)
            : base(application, factories)
        {
        }

        public override IBundleContainer Create(IEnumerable<Bundle> unprocessedBundles)
        {
            // The bundles may get altered, so force the evaluation of the enumerator first.
            var bundlesArray = unprocessedBundles.ToArray();

            ProcessAllBundles(bundlesArray);

            var externalBundles = CreateExternalBundlesFromReferences(bundlesArray);

            return new BundleContainer(bundlesArray.Concat(externalBundles));
        }
    }
}