using System.IO;
using Cassette.BundleProcessing;
using Cassette.Utilities;
using Moq;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette.Stylesheets
{
	public class LessBundlePipelineModifier_Tests
	{
	    readonly StylesheetPipeline originalPipeline;
	    readonly IBundlePipeline<StylesheetBundle> modifiedPipeline;

	    public LessBundlePipelineModifier_Tests()
	    {
            var minifier = Mock.Of<IStylesheetMinifier>();
            var urlGenerator = Mock.Of<IUrlGenerator>();
            var compiler = new Mock<ILessCompiler>();
	        var settings = new CassetteSettings();

	        var container = new TinyIoCContainer();
	        container.Register(compiler.Object);
	        container.Register(minifier);
	        container.Register(urlGenerator);
	        container.Register(settings);

            originalPipeline = new StylesheetPipeline(container, settings);
            var modifier = new LessBundlePipelineModifier();
            modifiedPipeline = modifier.Modify(originalPipeline);
	    }

        [Fact]
        public void ModifiedPipelineIsSameObjectAsOriginalPipeline()
        {
            modifiedPipeline.ShouldBeSameAs(originalPipeline);
        }

	    [Fact]
	    public void WhenModifiedPipelineProcessesBundle_ThenReferenceInLessAssetIsParsed()
	    {
	        var asset = new Mock<IAsset>();
	        asset.SetupGet(a => a.Path).Returns("~/file.less");
            asset.Setup(a => a.OpenStream()).Returns(() => "// @reference 'other.less';".AsStream());
            var bundle = new StylesheetBundle("~");
	        bundle.Assets.Add(asset.Object);

	        modifiedPipeline.Process(bundle);

            asset.Verify(a => a.AddReference("other.less", 1));
	    }

	    [Fact]
        public void WhenModifiedPipelineProcessesBundle_ThenLessAssetHasCompileAssetTransformAdded()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/file.less");
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            var bundle = new StylesheetBundle("~");
            bundle.Assets.Add(asset.Object);

            modifiedPipeline.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(t => t is CompileAsset)));
        }
	}
}