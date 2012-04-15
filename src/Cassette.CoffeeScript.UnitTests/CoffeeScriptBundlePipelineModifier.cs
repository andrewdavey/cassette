using System;
using System.IO;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using TinyIoC;
using Xunit;

namespace Cassette.Scripts
{
    public class GivenPiplineWhereCompileCoffeeScriptWithCustomCompiler
    {
        readonly ScriptBundle bundle;
        readonly Mock<IAsset> asset;
        readonly IBundlePipeline<ScriptBundle> pipeline;
        readonly Mock<ICoffeeScriptCompiler> compiler;

        public GivenPiplineWhereCompileCoffeeScriptWithCustomCompiler()
        {
            bundle = new ScriptBundle("~/");
            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/test.coffee");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            bundle.Assets.Add(asset.Object);

            compiler = new Mock<ICoffeeScriptCompiler>();
            var settings = new CassetteSettings();
            var container = new TinyIoCContainer();
            container.Register(compiler.Object);
            container.Register(Mock.Of<IJavaScriptMinifier>());
            container.Register(Mock.Of<IUrlGenerator>());
            container.Register(settings);
            var modifier = new CoffeeScriptBundlePipelineModifier();
            pipeline = new ScriptPipeline(container, settings);
            pipeline = modifier.Modify(pipeline);
        }

        [Fact]
        public void WhenProcessBundleAndApplyTheAddedTransform_ThenCustomCompilerIsUsed()
        {
            CompileAsset transformer = null;
            ExpectCompileAssetTransformToBeAddedToAsset(t => transformer = t);

            pipeline.Process(bundle);

            ExpectCompileToBeCalled();
            ApplyTransform(transformer);
            compiler.VerifyAll();
        }

        void ExpectCompileAssetTransformToBeAddedToAsset(Action<CompileAsset> gotTransformer)
        {
            asset
                .Setup(a => a.AddAssetTransformer(It.IsAny<CompileAsset>()))
                .Callback<IAssetTransformer>(c =>
                {
                    var t = c as CompileAsset;
                    if (t != null) gotTransformer(t);
                });
        }

        void ExpectCompileToBeCalled()
        {
            compiler
                .Setup(c => c.Compile(It.IsAny<string>(), It.IsAny<CompileContext>()))
                .Returns(new CompileResult("", new string[0]))
                .Verifiable();
        }

        void ApplyTransform(CompileAsset transformer)
        {
            transformer.Transform(() => Stream.Null, asset.Object)().Dispose();
        }
    }
}