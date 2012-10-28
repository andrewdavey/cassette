using Cassette.Scripts;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.RequireJS
{
    public class PlainScriptTests
    {
        readonly Mock<IModuleInitializer> modules;

        public PlainScriptTests()
        {
            modules = new Mock<IModuleInitializer>();
        }

        [Fact]
        public void AliasIsFilenameWithoutExtension()
        {
            var asset = new StubAsset("~/test.js", "var test = {};");
            var bundle = new ScriptBundle("~");

            var module = new PlainScript(asset, bundle, modules.Object);

            module.Alias.ShouldEqual("test");
        }

        [Fact]
        public void AliasIsFromFilenameConvertsInvalidCharactersToUnderscores()
        {
            var asset = new StubAsset("~/test-test.js", "var test_test = {};");
            var bundle = new ScriptBundle("~");

            var module = new PlainScript(asset, bundle, modules.Object);

            module.Alias.ShouldEqual("test_test");
        }

        [Fact]
        public void AssetIsWrappedInDefineCall()
        {
            var asset = new StubAsset("~/test.js", "var test = {};");
            var bundle = new ScriptBundle("~");

            var module = new PlainScript(asset, bundle, modules.Object);

            var output = asset.GetTransformedContent();
            output.ShouldEqual("define(\"test\",[],function(){var test = {};\r\nreturn test;});");
        }

        [Fact]
        public void DefineCallReturnsModuleReturnExpression()
        {
            var asset = new StubAsset("~/test.js", "var test = {};");
            var bundle = new ScriptBundle("~");

            var module = new PlainScript(asset, bundle, modules.Object)
            {
                ModuleReturnExpression = "{}"
            };

            var output = asset.GetTransformedContent();
            output.ShouldEqual("define(\"test\",[],function(){var test = {};\r\nreturn {};});");
        }

        [Fact]
        public void GivenScriptHasNoTopLevelVarOrReturnExpressionThenDefineHasNoReturn()
        {
            var asset = new StubAsset("~/test.js");
            var bundle = new ScriptBundle("~");

            var module = new PlainScript(asset, bundle, modules.Object);

            var output = asset.GetTransformedContent();
            output.ShouldEqual("define(\"test\",[],function(){\r\n});");
        }

        [Fact]
        public void AssetReferencesAreTranslatedIntoModuleDependencies()
        {
            SetupFakeModule("foo");
            SetupFakeModule("bar");

            var asset = new StubAsset("~/test.js");
            asset.ReferenceList.Add(new AssetReference("~/test.js", "~/foo.js", 1, AssetReferenceType.SameBundle));
            asset.ReferenceList.Add(new AssetReference("~/test.js", "~/bar.js", 1, AssetReferenceType.SameBundle));
            var bundle = new ScriptBundle("~");

            var module = new PlainScript(asset, bundle, modules.Object);

            var output = asset.GetTransformedContent();
            output.ShouldEqual("define(\"test\",[\"foo\",\"bar\"],function(foo,bar){\r\n});");
        }

        void SetupFakeModule(string name)
        {
            modules
                .SetupGet(m => m["~/" + name + ".js"])
                .Returns(FakeModule(name));
        }

        IAmdModule FakeModule(string path)
        {
            var module = new Mock<IAmdModule>();
            module.SetupGet(m => m.Alias).Returns(path);
            module.SetupGet(m => m.ModulePath).Returns(path);
            return module.Object;
        }
    }
}