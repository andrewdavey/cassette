using System.Linq;
using Cassette.BundleProcessing;
using Cassette.Utilities;
using Moq;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptPipeline_Tests
    {
        readonly ScriptPipeline pipeline;
        readonly IJavaScriptMinifier minifier;
        readonly IUrlGenerator urlGenerator;
        readonly CassetteSettings settings;

        public ScriptPipeline_Tests()
        {
            minifier = Mock.Of<IJavaScriptMinifier>();
            urlGenerator = Mock.Of<IUrlGenerator>();
            var container = new TinyIoCContainer();
            settings = new CassetteSettings();
            container.Register(minifier);
            container.Register(urlGenerator);
            container.Register(settings);
            pipeline = new ScriptPipeline(container, settings);
        }

        [Fact]
        public void GivenProductionMode_WhenProcessBundle_ThenRendererIsScriptBundleHtmlRenderer()
        {
            settings.IsDebuggingEnabled = false;

            var bundle = new ScriptBundle("~/test");

            pipeline.Process(bundle);

            bundle.Renderer.ShouldBeType<ScriptBundleHtmlRenderer>();
        }

        [Fact]
        public void GivenDebugMode_WhenProcessBundle_ThenRendererIsDebugScriptBundleHtmlRenderer()
        {
            settings.IsDebuggingEnabled = true;

            var bundle = new ScriptBundle("~/test");

            pipeline.Process(bundle);

            bundle.Renderer.ShouldBeType<DebugScriptBundleHtmlRenderer>();
        }

        [Fact]
        public void GivenNotDebugMode_ThenConcatenateAssetsWithSemicolonSeparator()
        {
            var step = pipeline.OfType<ConcatenateAssets>().First();
            step.Separator.ShouldEqual(";");
        }

        [Fact]
        public void WhenProcessBundle_ThenHashIsAssigned()
        {
            var bundle = new ScriptBundle("~");

            pipeline.Process(bundle);

            bundle.Hash.ShouldNotBeNull();
        }

        Mock<IAsset> StubAsset(string filename = null, string content = "")
        {
            var asset = new Mock<IAsset>();
            asset.Setup(f => f.OpenStream())
                .Returns(() => content.AsStream());
            asset.SetupGet(a => a.Path)
                .Returns(filename ?? "~/test.js");
            return asset;
        }
    }
}