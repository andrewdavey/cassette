using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.Scripts;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.RequireJS
{
    public class AutoAmdModuleTransformTests
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
            var jquery = new Mock<IAmdModule>();
            jquery.SetupGet(m => m.Alias).Returns("$");
            jquery.SetupGet(m => m.ModulePath).Returns("jquery");
            modules.SetupGet(m => m["~/vendor/jquery.js"])
                   .Returns(jquery.Object);
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
                .Returns(() =>
                {
                    var module = new Mock<IAmdModule>();
                    module.SetupGet(m => m.Alias).Returns(Path.GetFileNameWithoutExtension(reference));
                    module.SetupGet(m => m.ModulePath).Returns(PathHelpers.ConvertCassettePathToModulePath(reference));
                    return module.Object;
                });
        }

        void AssertTransform(string path, string input, string expectedOutput)
        {
            var asset = new StubAsset(path);
            asset.ReferenceList.AddRange(references.Select(r => new AssetReference("~/", r, 1, AssetReferenceType.SameBundle)));

            var module = new AutoAmdModule(asset, new ScriptBundle("~"), new SimpleJsonSerializer(), modules.Object);
            var outputStreamFactory = module.Transform(() => input.AsStream(), null);
            var output = outputStreamFactory().ReadToEnd();
            output.ShouldEqual(expectedOutput);
        }
    }
}