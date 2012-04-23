using System.IO;
using Cassette.BundleProcessing;
using Moq;
using Should;
using Xunit;
using TinyIoC;

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
        public void GivenCompileCoffeeScriptIsFalse_WhenProcessBundle_ThenCompileAssetTransformerNotAddedToAsset()
        {
            var bundle = new ScriptBundle("~");
            var asset = StubCoffeeScriptAsset();
            bundle.Assets.Add(asset.Object);
            
            pipeline.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.IsAny<CompileAsset>()), Times.Never());
        }

        [Fact]
        public void WhenProcessBundle_ThenHashIsAssigned()
        {
            var bundle = new ScriptBundle("~");

            pipeline.Process(bundle);

            bundle.Hash.ShouldNotBeNull();
        }

        Mock<IAsset> StubCoffeeScriptAsset()
        {
            var asset = new Mock<IAsset>();
            asset.Setup(f => f.OpenStream())
                .Returns(Stream.Null);
            asset.SetupGet(a => a.Path)
                .Returns("~/test.coffee");
            return asset;
        }
    }
}