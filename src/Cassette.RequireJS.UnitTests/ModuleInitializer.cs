using System;
using System.Collections.Generic;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette.RequireJS
{
    public class ModuleInitializerTests
    {
        readonly ModuleInitializer configuration;
        readonly List<Bundle> bundles;
        
        public ModuleInitializerTests()
        {
            bundles = new List<Bundle>();
            configuration = new ModuleInitializer(Mock.Of<IConfigurationScriptBuilder>());
            GivenBundle("~/shared", new StubAsset("~/shared/require.js"));
        }

        [Fact]
        public void InitializeModulesFromBundlesAssignsMainBundlePath()
        {
            configuration.InitializeModules(bundles, "~/shared/require.js");
            configuration.MainBundlePath.ShouldEqual("~/shared");
        }

        [Fact]
        public void GivenRequireJsPathNotFoundThenInitializeModulesFromBundlesThrows()
        {
            Assert.Throws<ArgumentException>(
                () => configuration.InitializeModules(bundles, "~/fail/require.js")
            );
        }

        [Fact]
        public void SetImportAliasSetsModuleAlias()
        {
            GivenBundle(
                "~/shared/jquery.js", 
                new StubAsset("~/shared/jquery.js", "define(\"jquery\",[],function(){})")
            );

            configuration.InitializeModules(bundles, "~/shared/require.js");
            configuration.SetImportAlias("~/shared/jquery.js", "$");

            configuration["~/shared/jquery.js"].Alias.ShouldEqual("$");
        }

        [Fact]
        public void GivenUnknownScriptPathThenSetImportAliasThrowsArgumentException()
        {
            configuration.InitializeModules(bundles, "~/shared/require.js");

            Assert.Throws<ArgumentException>(
                () => configuration.SetImportAlias("~/shared/notfound.js", "$")
            );
        }

        [Fact]
        public void SetModuleReturnExpressionChangesPlainScriptModuleReturnExpression()
        {
            GivenBundle("~", new StubAsset("~/test.js"));
            configuration.InitializeModules(bundles, "~/shared/require.js");
            configuration.SetModuleReturnExpression("~/test.js", "{ test: 1 }");
            ((PlainScript)configuration["~/test.js"]).ModuleReturnExpression.ShouldEqual("{ test: 1 }");
        }

        [Fact]
        public void GivenNamedBundleThenSetModuleReturnExpressionThrowsException()
        {
            GivenBundle("~", new StubAsset("~/test.js", "define(\"test\",[],function(){})"));
            configuration.InitializeModules(bundles, "~/shared/require.js");
            Assert.Throws<ArgumentException>(
                () => configuration.SetModuleReturnExpression("~/test.js", "{ test: 1 }")
            );
        }

        void GivenBundle(string path, params IAsset[] assets)
        {
            var bundle = new ScriptBundle(path);
            foreach (var asset in assets)
            {
                bundle.Assets.Add(asset);
            }
            bundles.Add(bundle);
        }
    }
}