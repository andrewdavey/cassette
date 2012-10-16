using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette.RequireJS.Tests
{
    public class DefineCallTransformerTests
    {
        [Fact]
        public void WrapWithModuleDefine()
        {
            AssertTransform(
                "~/foo.js",
                "var foo = {};",
                "define(\"foo\",[],function(){var foo = {};\r\n" +
                "return foo;});"
            );
        }

        [Fact]
        public void ModulePathIsAssetPathWithoutPrefixAndExtension()
        {
            AssertTransform(
                "~/scripts/foo.js",
                "var foo = {};",
                "define(\"scripts/foo\",[],function(){var foo = {};\r\n" +
                "return foo;});"
            );
        }

        [Fact]
        public void ModuleReturnVarIsDeterminedByFilename()
        {
            AssertTransform(
                "~/scripts/module.js",
                "var module = {};",
                "define(\"scripts/module\",[],function(){var module = {};\r\n" +
                "return module;});"
            );
        }

        [Fact]
        public void GivenFilenameWithHyphensThenExportedVariableNameHasUnderscores()
        {
            AssertTransform(
                "~/scripts/module-test.js",
                "var module_test = {};",
                "define(\"scripts/module-test\",[],function(){var module_test = {};\r\n" +
                "return module_test;});"
            );
        }

        [Fact]
        public void GivenFilenameStartsWith2ThenExportedVariableNameIsPrefixedWithUnderscore()
        {
            AssertTransform(
                "~/scripts/2module.js",
                "var _2module = {};",
                "define(\"scripts/2module\",[],function(){var _2module = {};\r\n" +
                "return _2module;});"
            );
        }

        void AssertTransform(string path, string input, string expectedOutput)
        {
            var asset = new StubAsset(
                fullPath: path,
                content: input
            );
            var transformer = new DefineCallTransformer(new SimpleJsonSerializer());
            asset.AddAssetTransformer(transformer);
            var output = asset.OpenStream().ReadToEnd();
            output.ShouldEqual(expectedOutput);
        }
    }
}