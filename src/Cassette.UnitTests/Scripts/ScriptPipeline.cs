using System.IO;
using Cassette.BundleProcessing;
using Cassette.IO;
using Moq;
using Should;
using Xunit;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    public class ScriptPipeline_Tests
    {
        [Fact]
        public void CompileCoffeeScriptDefaultsToTrue()
        {
            var pipeline = new ScriptPipeline();
            pipeline.CompileCoffeeScript.ShouldBeTrue();
        }

        [Fact]
        public void GivenProductionMode_WhenProcessBundle_ThenRendererIsScriptBundleHtmlRenderer()
        {
            var settings = new CassetteSettings("") { IsDebuggingEnabled = false };

            var bundle = new ScriptBundle("~/test");

            var pipeline = new ScriptPipeline();
            pipeline.Process(bundle, settings);

            bundle.Renderer.ShouldBeType<ScriptBundleHtmlRenderer>();
        }

        [Fact]
        public void GivenDebugMode_WhenProcessBundle_ThenRendererIsDebugScriptBundleHtmlRenderer()
        {
            var settings = new CassetteSettings("") { IsDebuggingEnabled = true };

            var bundle = new ScriptBundle("~/test");

            var pipeline = new ScriptPipeline();
            pipeline.Process(bundle, settings);

            bundle.Renderer.ShouldBeType<DebugScriptBundleHtmlRenderer>();
        }

        [Fact]
        public void GivenCompileCoffeeScriptIsFalse_WhenProcessBundle_ThenCompileAssetTransformerNotAddedToAsset()
        {
            var pipeline = new ScriptPipeline { CompileCoffeeScript = false };
            var bundle = new ScriptBundle("~");
            var asset = StubCoffeeScriptAsset();
            bundle.Assets.Add(asset.Object);
            
            pipeline.Process(bundle, new CassetteSettings(""));

            asset.Verify(a => a.AddAssetTransformer(It.IsAny<CompileAsset>()), Times.Never());
        }

        [Fact]
        public void GivenCompileCoffeeScriptIsTrue_WhenProcessBundle_ThenCompileAssetTransformerIsAddedToAsset()
        {
            var pipeline = new ScriptPipeline { CompileCoffeeScript = true };
            var bundle = new ScriptBundle("~");
            var asset = StubCoffeeScriptAsset();
            bundle.Assets.Add(asset.Object);

            pipeline.Process(bundle, new CassetteSettings(""));

            asset.Verify(a => a.AddAssetTransformer(It.IsAny<CompileAsset>()));
        }

        [Fact]
        public void WhenProcessBundle_ThenHashIsAssigned()
        {
            var pipeline = new ScriptPipeline();
            var bundle = new ScriptBundle("~");

            pipeline.Process(bundle, new CassetteSettings(""));

            bundle.Hash.ShouldNotBeNull();
        }

        static Mock<IAsset> StubCoffeeScriptAsset()
        {
            var asset = new Mock<IAsset>();
            var file = new Mock<IFile>();
            file.SetupGet(f => f.FullPath)
                .Returns("~/test.coffee");
            asset.Setup(f => f.OpenStream())
                .Returns(Stream.Null);
            asset.SetupGet(a => a.SourceFile)
                .Returns(file.Object);
            return asset;
        }
    }
}