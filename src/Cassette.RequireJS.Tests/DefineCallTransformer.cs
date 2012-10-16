using System.Collections.Generic;
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

        [Fact]
        public void GivenAssetReferenceThenModuleHasDependencyOnTheReferencedAsset()
        {
            GivenReference(new AssetReference("~/foo.js", "~/bar.js", 1, AssetReferenceType.SameBundle));
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
            GivenReference(new AssetReference("~/app/foo.js", "~/vendor/jquery.js", 1, AssetReferenceType.DifferentBundle));
            GivenConfiguration(new AmdConfiguration
            {
                {"~/vendor/jquery.js", "jquery", "$"}
            });
            AssertTransform(
                "~/app/foo.js",
                "var foo = {};",
                "define(\"app/foo\",[\"jquery\"],function($){var foo = {};\r\n" +
                "return foo;});");
        }

        readonly List<AssetReference> references = new List<AssetReference>();
        AmdConfiguration amdConfiguration = new AmdConfiguration();

        void GivenReference(AssetReference assetReference)
        {
            references.Add(assetReference);
        }

        void GivenConfiguration(AmdConfiguration newAmdConfiguration)
        {
            amdConfiguration = newAmdConfiguration;
        }

        void AssertTransform(string path, string input, string expectedOutput)
        {
            var asset = new StubAsset(path, input);
            asset.ReferenceList.AddRange(references);
            var transformer = new DefineCallTransformer(amdConfiguration, new SimpleJsonSerializer());
            asset.AddAssetTransformer(transformer);
            var output = asset.OpenStream().ReadToEnd();
            output.ShouldEqual(expectedOutput);
        }
    }
}