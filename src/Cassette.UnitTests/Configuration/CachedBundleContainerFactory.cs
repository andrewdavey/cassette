using System.Collections.Generic;
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
                    new ScriptBundleManifest
                    {
                        Path = "~",
                        Hash = new byte[] { 1, 2, 3 },
                        Html = () => ""
                    }
                }
            );
            cache.Setup(c => c.LoadCassetteManifest()).Returns(cachedManifest);

            var urlModifier = Mock.Of<IUrlModifier>();
            var scriptBundle = new ScriptBundle("~") { Renderer = new ConstantHtmlRenderer<ScriptBundle>("", urlModifier) };
            var factory = CreateFactory(new[] { scriptBundle });
            var container = factory.CreateBundleContainer();

            var bundle = container.Bundles.Single();
            bundle.Hash.ShouldEqual(new byte[] { 1, 2, 3 });
        }

        internal override IBundleContainerFactory CreateFactory(IEnumerable<Bundle> bundles)
        {
            return new CachedBundleContainerFactory(new BundleCollection(Settings, bundles), cache.Object, Settings);
        }
    }
}