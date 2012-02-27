using System.Collections.Generic;

namespace Cassette.Configuration
{
    public class BundleContainerFactory_Tests : BundleContainerFactoryTestSuite
    {
        internal override IBundleContainerFactory CreateFactory(IEnumerable<Bundle> bundles)
        {
            return new BundleContainerFactory(new BundleCollection(Settings, bundles), Settings);
        }
    }
}