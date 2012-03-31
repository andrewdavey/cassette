using System.Linq;

namespace Cassette.Configuration
{
    class BundleContainerFactory : BundleContainerFactoryBase
    {
        readonly BundleCollection bundles;

        public BundleContainerFactory(BundleCollection bundles, CassetteSettings settings, IBundleFactoryProvider bundleFactoryProvider)
            : base(settings, bundleFactoryProvider)
        {
            this.bundles = bundles;
        }

        public override IBundleContainer CreateBundleContainer()
        {
            ProcessAllBundles(bundles);
            var externalBundles = CreateExternalBundlesUrlReferences(bundles);
            return new BundleContainer(bundles.Concat(externalBundles));
        }
    }
}