using System;

namespace Cassette
{
    class BundleFactoryProvider : IBundleFactoryProvider
    {
        readonly Func<Type, IBundleFactory<Bundle>> getBundleFactoryForBundleType;

        public BundleFactoryProvider(Func<Type, IBundleFactory<Bundle>> getBundleFactoryForBundleType)
        {
            this.getBundleFactoryForBundleType = getBundleFactoryForBundleType;
        }

        public IBundleFactory<T> GetBundleFactory<T>() where T : Bundle
        {
            return (IBundleFactory<T>)getBundleFactoryForBundleType(typeof(T));
        }

        public IBundleFactory<Bundle> GetBundleFactory(Type bundleType)
        {
            return getBundleFactoryForBundleType(bundleType);
        }
    }
}