using System;
using System.Collections.Generic;

namespace Cassette.Configuration
{
    public class BundleContainerFactory_Tests : BundleContainerFactoryTestSuite
    {
        internal override IBundleContainerFactory CreateFactory(IDictionary<Type, IBundleFactory<Bundle>> factories)
        {
            return new BundleContainerFactory(factories, settings);
        }
    }
}