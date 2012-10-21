using System;
using System.IO;
using System.Linq;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette.RequireJS
{
    public class AmdConfigurationTests
    {
        readonly AmdConfiguration configuration;
        readonly BundleCollection bundles;
        readonly Mock<IAssetTransformer> moduleWrapper;
        readonly Mock<IAssetTransformer> modulePathInserter;
        readonly Mock<IAssetTransformer> moduleShimmer;

        public AmdConfigurationTests()
        {
            moduleWrapper = FakeAssetTransformer();
            modulePathInserter = FakeAssetTransformer();
            moduleShimmer = FakeAssetTransformer();
            bundles = new BundleCollection(new CassetteSettings(Enumerable.Empty<IConfiguration<CassetteSettings>>()), null, null);
            configuration = new AmdConfiguration(bundles, () => moduleWrapper.Object, () => modulePathInserter.Object, _ => moduleShimmer.Object);
        }

        [Fact]
        public void ModulePerAssetAddsDefineWrapperTransformerToAssetInBundle()
        {
            var bundle = new ScriptBundle("~/bundle");
            var asset = new StubAsset("~/bundle/test.js");
            bundle.Assets.Add(asset);
            bundles.Add(bundle);

            configuration.ModulePerAsset("~/bundle");

            AssetIsTransformed(asset, moduleWrapper);
        }

        [Fact]
        public void WhenAddModuleThenModulePathInserterWillTransformTheScript()
        {
            var bundle = new ScriptBundle("~/bundle");
            var asset = new StubAsset("~/bundle/jquery.js");
            bundle.Assets.Add(asset);
            bundles.Add(bundle);

            configuration.AddModule("~/bundle/jquery.js");

            AssetIsTransformed(asset, modulePathInserter);
        }

        [Fact]
        public void AddedModulesAvailableViaIndexer()
        {
            var bundle = new ScriptBundle("~/bundle");
            var asset = new StubAsset("~/bundle/test.js");
            bundle.Assets.Add(asset);
            bundles.Add(bundle);

            configuration.ModulePerAsset("~/bundle");

            var module = configuration["~/bundle/test.js"];
            module.Bundle.ShouldBeSameAs(bundle);
            module.Asset.ShouldBeSameAs(asset);
            module.ModulePath.ShouldEqual("bundle/test");
            module.Alias.ShouldEqual("test");
        }

        [Fact]
        public void WhenAddModuleUsingShimThenScriptIsTransformedUsingModuleShimmer()
        {
            var bundle = new ScriptBundle("~/bundle");
            var asset = new StubAsset("~/bundle/test.js");
            bundle.Assets.Add(asset);
            bundles.Add(bundle);

            configuration.AddModuleUsingShim("~/bundle/test.js", new Shim { ModuleReturnExpression = "test" });

            AssetIsTransformed(asset, moduleShimmer);
        }

        Mock<IAssetTransformer> FakeAssetTransformer()
        {
            var fake = new Mock<IAssetTransformer>();
            fake.Setup(w => w.Transform(It.IsAny<Func<Stream>>(), It.IsAny<IAsset>()))
                .Returns(() => Stream.Null);
            return fake;
        }

        void AssetIsTransformed(IAsset asset, Mock<IAssetTransformer> mock)
        {
            using (asset.OpenStream())
            {
                mock.Verify(w => w.Transform(It.IsAny<Func<Stream>>(), asset));
            }
        }
    }
}