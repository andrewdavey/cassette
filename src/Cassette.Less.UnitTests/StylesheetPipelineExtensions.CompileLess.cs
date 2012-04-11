using Cassette.BundleProcessing;
using Cassette.Configuration;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;
using System.IO;

namespace Cassette.Stylesheets
{
	public class StylesheetPipelineExtensions_CompileLess_Tests
	{
        [Fact]
        public void CompileLessReturnsTheSamePipeline()
        {
            var minifier = Mock.Of<IStylesheetMinifier>();
            var urlGenerator = Mock.Of<IUrlGenerator>();
            var pipeline = new StylesheetPipeline(minifier, urlGenerator);
            var returnedPipeline = pipeline.CompileLess();
            returnedPipeline.ShouldBeSameAs(pipeline);
        }

        [Fact]
        public void GivenCompileLessIsTrue_WhenProcessBundle_ThenLessAssetHasCompileAssetTransformAdded()
        {
            var minifier = new Mock<IStylesheetMinifier>();
            var urlGenerator = new Mock<IUrlGenerator>();
            var pipeline = new StylesheetPipeline(minifier.Object, urlGenerator.Object);

            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/file.less");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            pipeline.CompileLess();
            pipeline.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CompileAsset)));
        }
	}

    public class StylesheetPipelineWhereLessCompilerTrue
    {
        public StylesheetPipelineWhereLessCompilerTrue()
        {
            var minifier = new MicrosoftStylesheetMinifier();
            var urlGenerator = new Mock<IUrlGenerator>();
            pipeline = new StylesheetPipeline(minifier, urlGenerator.Object);

            settings = new CassetteSettings();
            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("asset.less");
            asset.Setup(a => a.OpenStream()).Returns(() => "// @reference 'other.less';".AsStream());
            bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);
            pipeline.CompileLess();
        }

        readonly Mock<IAsset> asset;
        readonly StylesheetPipeline pipeline;
        readonly CassetteSettings settings;
        readonly StylesheetBundle bundle;

        [Fact]
        public void ReferenceInLessAssetIsParsed()
        {
            pipeline.Process(bundle, settings);
            asset.Verify(a => a.AddReference("other.less", 1));
        }

        [Fact]
        public void LessAssetIsCompiled()
        {
            pipeline.Process(bundle, settings);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(
                t => t is CompileAsset
            )));
        }
    }
}