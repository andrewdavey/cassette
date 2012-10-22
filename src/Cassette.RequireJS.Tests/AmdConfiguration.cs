using System.Linq;
using Cassette.Scripts;
using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette.RequireJS
{
    public class AmdConfigurationTests
    {
        readonly AmdConfiguration configuration;
        readonly BundleCollection bundles;
        
        public AmdConfigurationTests()
        {
            bundles = new BundleCollection(new CassetteSettings(Enumerable.Empty<IConfiguration<CassetteSettings>>()), null, null);
            configuration = new AmdConfiguration(bundles, new SimpleJsonSerializer());
        }

        [Fact]
        public void ModulePerAssetAddsDefineWrapperTransformerToAssetInBundle()
        {
            var bundle = new ScriptBundle("~/bundle");
            var asset = new StubAsset("~/bundle/test.js", "var test = {};");
            bundle.Assets.Add(asset);
            bundles.Add(bundle);

            configuration.ModulePerAsset("~/bundle");

            AssetIsTransformed(
                asset,
                "define(\"bundle/test\",[],function(){var test = {};\r\nreturn test;});"
            );
        }

        [Fact]
        public void WhenAddModuleThenModulePathInserterWillTransformTheScript()
        {
            var bundle = new ScriptBundle("~/bundle");
            var asset = new StubAsset("~/bundle/knockout.js", "define([],function(){})");
            bundle.Assets.Add(asset);
            bundles.Add(bundle);

            configuration.AddModule("~/bundle/knockout.js", "ko");

            AssetIsTransformed(
                asset,
                "define(\"bundle/knockout\",[],function(){})"
            );
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
            module.ModulePath.ShouldEqual("bundle/test");
            module.Alias.ShouldEqual("test");
        }

        [Fact]
        public void WhenAddModuleUsingShimThenScriptIsTransformedUsingModuleShimmer()
        {
            var bundle = new ScriptBundle("~/bundle");
            var asset = new StubAsset("~/bundle/test.js", "var test = {};");
            bundle.Assets.Add(asset);
            bundles.Add(bundle);

            configuration.AddModuleUsingShim("~/bundle/test.js", "test");

            AssetIsTransformed(
                asset,
                "var test = {};\r\ndefine(\"bundle/test\",[],function(){return test;});"
            );
        }

        void AssetIsTransformed(IAsset asset, string expectedOutput)
        {
            asset.OpenStream().ReadToEnd().ShouldEqual(expectedOutput);
        }
    }
}