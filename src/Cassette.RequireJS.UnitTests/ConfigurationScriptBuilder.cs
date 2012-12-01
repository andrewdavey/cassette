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

        void GivenBundle(string path, params IAsset[] assets)
        {
            var bundle = new ScriptBundle(path);
            foreach (var asset in assets)
            {
                bundle.Assets.Add(asset);
            }
            bundles.Add(bundle);
        }

        void WhenBuildScriptForRelease()
        {
            BuildScript(false);
        }

        void WhenBuildScriptForDebug()
        {
            BuildScript(true);
        }

        void BuildScript(bool debug)
        {
            modules.InitializeModules(bundles, "~/shared/required.js");
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
                var offset = "requirejs.config(".Length;
                var configObjectScript = script.Substring(offset, script.Length - offset - 2);
                var config = JsonConvert.DeserializeObject<RequireJsConfig>(configObjectScript);
                return config.paths;
            }
        }

        public class RequireJsConfig
        {
            public Dictionary<string, string> paths { get; set; }
        }
    }
}