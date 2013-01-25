using Cassette.Scripts;
using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette.RequireJS
{
    public class AnonymousModuleTests
    {
        [Fact]
        public void ModulePathIsBuiltFromAssetPath()
        {
            var asset = new StubAsset("~/test.js");
            var bundle = new ScriptBundle("~");

            var module = new AnonymousModule(asset, bundle);
            module.ModulePath.ShouldEqual("test");
        }

        [Fact]
        public void AssetIsTransformedToIncludeModulePathInDefineCall()
        {
            var asset = new StubAsset("~/test.js", "define([],function(){})");
            var bundle = new ScriptBundle("~");

            var module = new AnonymousModule(asset, bundle);

            var output = asset.GetTransformedContent();
            output.ShouldEqual("define(\"test\",[],function(){})");
        }

        [Fact]
        public void AssetIsTransformedToIncludeModulePathInDefineCallWithNoDependencies()
        {
            var asset = new StubAsset("~/test.js", "define(function(){})");
            var bundle = new ScriptBundle("~");

            var module = new AnonymousModule(asset, bundle);

            var output = asset.GetTransformedContent();
            output.ShouldEqual("define(\"test\",function(){})");
        }

        [Fact]
        public void AssetIsNotMinified()
        {
            var asset = new StubAsset("~/test.js", "define([],function(){var x = 1;})");
            var bundle = new ScriptBundle("~");

            var module = new AnonymousModule(asset, bundle);

            asset.GetTransformedContent().ShouldEqual("define(\"test\",[],function(){var x = 1;})");
        }
    }
}