using System;

namespace Cassette
{
    public interface IBundleFactoryProvider
    {
        IBundleFactory<T> GetBundleFactory<T>() where T : Bundle;
        IBundleFactory<Bundle> GetBundleFactory(Type bundleType);
    }
}