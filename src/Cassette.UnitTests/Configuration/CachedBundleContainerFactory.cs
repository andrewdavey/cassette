using System.Linq;
using Cassette.Manifests;
using Cassette.Scripts;
using Cassette.Scripts.Manifests;
using Moq;
using Should;
using Xunit;

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

        [Fact]
        public void GivenCachedManifestWithOneBundle_WhenCreateWithMatchingUpToDateBundle_ThenCachedBundleIsReturned()
        {
            var cachedManifest = new CassetteManifest(
                "",
                new[]
                {
                    new ScriptBundleManifest { Path = "~", Hash = new byte[] { 1, 2, 3 } }
                }
            );
            cache.Setup(c => c.LoadCassetteManifest()).Returns(cachedManifest);

            var factory = CreateFactory();
            var container = factory.Create(new[] { new ScriptBundle("~") });

            var bundle = container.Bundles.Single();
            bundle.Hash.ShouldEqual(new byte[] { 1, 2, 3 });
            bundle.IsFromCache.ShouldBeTrue();
            bundle.IsProcessed.ShouldBeTrue();
        }

        internal override IBundleContainerFactory CreateFactory()
        {
            return new CachedBundleContainerFactory(cache.Object, Settings);
        }
    }
}