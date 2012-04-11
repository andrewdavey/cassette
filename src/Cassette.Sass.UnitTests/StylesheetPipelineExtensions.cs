#if !NET35
using Cassette.BundleProcessing;
using Cassette.Utilities;
using Moq;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetPipelineWithSassCompilation : StylesheetPipeline_Process_TestBase
    {
        public StylesheetPipelineWithSassCompilation()
        {
            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("asset.scss");
            asset.Setup(a => a.OpenStream()).Returns(() => "// @reference 'other.scss';".AsStream());
            bundle.Assets.Add(asset.Object);
            pipeline.CompileSass();
        }

        readonly Mock<IAsset> asset;

        [Fact]
        public void ReferenceInSassAssetIsParsed()
        {
            pipeline.Process(bundle, settings);
            asset.Verify(a => a.AddReference("other.scss", 1));
        }

        [Fact]
        public void SassAssetIsCompiled()
        {
            pipeline.Process(bundle, settings);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(
                t => t is CompileAsset
            )));
        }
    }
}
#endif