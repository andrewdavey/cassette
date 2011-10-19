using System;
using System.Collections.Generic;

namespace Cassette.Configuration
{
    public class BundleContainerFactory_Tests : BundleContainerFactoryTestSuite<BundleContainerFactory>
    {
        protected override BundleContainerFactory CreateFactory(IDictionary<Type, IBundleFactory<Bundle>> factories)
        {
            return new BundleContainerFactory(factories);
        }
    }
}