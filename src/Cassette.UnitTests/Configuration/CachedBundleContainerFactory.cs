using Cassette.Manifests;
using Moq;

namespace Cassette.Configuration
{
    public class CachedBundleContainerFactory_Tests : BundleContainerFactoryTestSuite
    {
        readonly Mock<ICassetteManifestCache> cache;

        public CachedBundleContainerFactory_Tests()
        {
            cache = new Mock<ICassetteManifestCache>();
            cache.Setup(c => c.LoadCassetteManifest()).Returns(() => new CassetteManifest());
        }

        internal override IBundleContainerFactory CreateFactory()
        {
            return new CachedBundleContainerFactory(cache.Object, Settings);
        }
    }
}