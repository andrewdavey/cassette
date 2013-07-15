using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Scripts;
using Moq;
using Newtonsoft.Json;
using Should;
using Xunit;

namespace Cassette.RequireJS
{
    public class ConfigurationScriptBuilderTests
    {
        readonly Mock<IUrlGenerator> urlGenerator;
        readonly List<Bundle> bundles = new List<Bundle>();
        readonly ModuleInitializer modules = new ModuleInitializer(Mock.Of<IConfigurationScriptBuilder>());
        string script;

        public ConfigurationScriptBuilderTests()
        {
            urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(g => g.CreateBundleUrl(It.IsAny<Bundle>())).Returns("BUNDLE-URL");
            urlGenerator.Setup(g => g.CreateAssetUrl(It.IsAny<IAsset>())).Returns("ASSET-URL");

            GivenBundle("~/shared", new StubAsset("~/shared/required.js"));
        }

        [Fact]    
        public void ConfigScriptCallsConfigFunction()
        {
            GivenBundle("~/app", new StubAsset("~/app/test.js"));
            WhenBuildScriptForRelease();
            script.ShouldStartWith("requirejs.config(");
            script.Last().ShouldEqual(';');
        }

        [Fact]
        public void ConfigScriptMapsModulePathToCassetteBundleUrl()
        {
            GivenBundle("~/app", new StubAsset("~/app/test.js"));
            WhenBuildScriptForRelease();
            Paths["app/test"].ShouldEqual("BUNDLE-URL?");
        }

        [Fact]
        public void EachAssetIsMappedInPaths()
        {
            GivenBundle(
                "~/app",
                new StubAsset("~/app/test1.js"),
                new StubAsset("~/app/test2.js")
                );
            WhenBuildScriptForRelease();
            Paths["app/test1"].ShouldEqual("BUNDLE-URL?");
            Paths["app/test2"].ShouldEqual("BUNDLE-URL?");
        }

        [Fact]
        public void MultipleBundlesAreMappedInPaths()
        {
            GivenBundle(
                "~/app1",
                new StubAsset("~/app1/test1.js"),
                new StubAsset("~/app1/test2.js")
                );
            GivenBundle(
                "~/app2",
                new StubAsset("~/app2/test1.js"),
                new StubAsset("~/app2/test2.js")
                );
            urlGenerator
                .Setup(g => g.CreateBundleUrl(It.IsAny<Bundle>()))
                .Returns<Bundle>(b => b.Path.Substring(1));

            WhenBuildScriptForRelease();

            Paths["app1/test1"].ShouldEqual("/app1?");
            Paths["app1/test2"].ShouldEqual("/app1?");
            Paths["app2/test1"].ShouldEqual("/app2?");
            Paths["app2/test2"].ShouldEqual("/app2?");
        }

        [Fact]
        public void GivenDebugThenConfigScriptMapsModulePathToCassetteAssetUrl()
        {
            GivenBundle("~/app", new StubAsset("~/app/test.js"));
            WhenBuildScriptForDebug();
            Paths["app/test"].ShouldEqual("ASSET-URL");
        }

        [Fact]
        public void AssetIsShimmed()
        {
            var asset1 = new StubAsset("~/app/test1.js");
            var asset2 = new StubAsset("~/app/test2.js");
            asset2.AddRawFileReference(asset1.Path);
            
            GivenBundle(
                "~/app",
                asset1,
                asset2
                );


            WhenBuildScriptForDebug(c => c.Shim("~/app/test2.js"));
            Paths["app/test1"].ShouldEqual("ASSET-URL");
            Paths["app/test2"].ShouldEqual("ASSET-URL");
            string dependency = Config.shim["app/test2"][0].ToString();
            dependency.ShouldEqual("app/test1");
        }

        [Fact]
        public void AssetIsShimmedWithExport()
        {
            var asset1 = new StubAsset("~/app/test1.js");
            var asset2 = new StubAsset("~/app/test2.js");
            asset2.AddRawFileReference(asset1.Path);

            GivenBundle(
                "~/app",
                asset1,
                asset2
                );


            WhenBuildScriptForDebug(c => c.Shim("~/app/test2.js","t2"));
            Paths["app/test1"].ShouldEqual("ASSET-URL");
            Paths["app/test2"].ShouldEqual("ASSET-URL");
            string dependency = Config.shim["app/test2"]["deps"][0].ToString();
            dependency.ShouldEqual("app/test1");

            string exports = Config.shim["app/test2"]["exports"].ToString();
            exports.ShouldEqual("t2");
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

        void WhenBuildScriptForRelease(Action<IModuleInitializer> configuration = null)
        {
            BuildScript(false, configuration);
        }

        void WhenBuildScriptForDebug(Action<IModuleInitializer> configuration = null)
        {
            BuildScript(true, configuration);
        }

        void BuildScript(bool debug, Action<IModuleInitializer> configuration = null)
        {
            modules.InitializeModules(bundles, "~/shared/required.js");
            if (configuration != null)
            {
                configuration(modules);
            }
            var builder = new ConfigurationScriptBuilder(
                urlGenerator.Object,
                new SimpleJsonSerializer(),
                debug
            );
            script = builder.BuildConfigurationScript(modules);
        }

        IDictionary<string, string> Paths
        {
            get
            {
                return Config.paths;
            }
        }

        RequireJsConfig Config
        {
            get
            {
                var offset = "requirejs.config(".Length;
                var configObjectScript = script.Substring(offset, script.Length - offset - 2);
                var config = JsonConvert.DeserializeObject<RequireJsConfig>(configObjectScript);
                return config;
            }
        }

        public class RequireJsConfig
        {
            public Dictionary<string, string> paths { get; set; }

            public Dictionary<string, dynamic> shim { get; set; }
        }
    }
}