using System.Linq;
using Cassette.Configuration;
using Cassette.IO;

namespace Cassette
{
    static class BundleFactoryExtensions
    {
        public static T CreateExternalBundle<T>(this IBundleFactory<T> bundleFactory, string url)
            where T : Bundle
        {
            return bundleFactory.CreateBundle(url, Enumerable.Empty<IFile>(), new BundleDescriptor
            {
                ExternalUrl = url
            });
        }

        public static T CreateExternalBundleWithLocalAssets<T>(this IBundleFactory<T> bundleFactory, string url, LocalAssetSettings settings)
            where T : Bundle
        {
            return bundleFactory.CreateBundle(url, Enumerable.Empty<IFile>(), new BundleDescriptor
            {
                ExternalUrl = url
            });
        }
    }
}