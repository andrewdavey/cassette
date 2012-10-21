using System.Collections.Generic;
using System.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette.RequireJS
{
    public class BareScriptToAmdModuleTransformerTests
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

        [Fact]
        public void GivenAssetReferenceThenModuleHasDependencyOnTheReferencedAsset()
        {
            GivenReference("~/bar.js");
            AssertTransform(
                "~/foo.js",
                "var foo = {};",
                "define(\"foo\",[\"bar\"],function(bar){var foo = {};\r\n" +
                "return foo;});"
            );
        }

        [Fact]
        public void GivenReferenceToVendorModuleThenDependencyUsesConfiguredModuleInfo()
        {
            GivenReference("~/vendor/jquery.js");
            modules.SetupGet(m => m["~/vendor/jquery.js"])
                   .Returns(new AmdModule("jquery", "$"));
            AssertTransform(
                "~/app/foo.js",
                "var foo = {};",
                "define(\"app/foo\",[\"jquery\"],function($){var foo = {};\r\n" +
                "return foo;});");
        }

        [Fact]
        public void GivenNoModuleVarThenDefineDoesNotReturnModuleVar()
        {
            AssertTransform("~/test.js", "", "define(\"test\",[],function(){\r\n});");
        }

        readonly List<string> references = new List<string>();
        readonly Mock<IAmdModuleCollection> modules = new Mock<IAmdModuleCollection>();

        void GivenReference(string reference)
        {
            references.Add(reference);
            modules
                .SetupGet(m => m[reference])
                .Returns(new AmdModule(
                   PathHelpers.ConvertCassettePathToModulePath(reference),
                   PathHelpers.ConvertCassettePathToModulePath(reference).Split('/').Last()
                ));
        }

        void AssertTransform(string path, string input, string expectedOutput)
        {
            var transformer = new BareScriptToAmdModuleTransformer(modules.Object, new SimpleJsonSerializer());
            var output = transformer.Transform(input, path, references);
            output.ShouldEqual(expectedOutput);
        }
    }
}