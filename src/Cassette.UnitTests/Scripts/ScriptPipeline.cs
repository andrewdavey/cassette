using System.IO;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptPipeline_Tests
    {
        readonly ScriptPipeline pipeline;
        readonly IJavaScriptMinifier minifier;
        readonly IUrlGenerator urlGenerator;

        public ScriptPipeline_Tests()
        {
            minifier = Mock.Of<IJavaScriptMinifier>();
            urlGenerator = Mock.Of<IUrlGenerator>();
            pipeline = new ScriptPipeline(minifier, urlGenerator);
        }

        [Fact]
        public void GivenProductionMode_WhenProcessBundle_ThenRendererIsScriptBundleHtmlRenderer()
        {
            var settings = new CassetteSettings() { IsDebuggingEnabled = false };

            var bundle = new ScriptBundle("~/test");

            pipeline.Process(bundle, settings);

            bundle.Renderer.ShouldBeType<ScriptBundleHtmlRenderer>();
        }

        [Fact]
        public void GivenDebugMode_WhenProcessBundle_ThenRendererIsDebugScriptBundleHtmlRenderer()
        {
            var settings = new CassetteSettings() { IsDebuggingEnabled = true };

            var bundle = new ScriptBundle("~/test");

            pipeline.Process(bundle, settings);

            bundle.Renderer.ShouldBeType<DebugScriptBundleHtmlRenderer>();
        }

        [Fact]
        public void GivenCompileCoffeeScriptIsFalse_WhenProcessBundle_ThenCompileAssetTransformerNotAddedToAsset()
        {
            var bundle = new ScriptBundle("~");
            var asset = StubCoffeeScriptAsset();
            bundle.Assets.Add(asset.Object);
            
            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.IsAny<CompileAsset>()), Times.Never());
        }

        [Fact]
        public void WhenProcessBundle_ThenHashIsAssigned()
        {
            var bundle = new ScriptBundle("~");

            pipeline.Process(bundle, new CassetteSettings());

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