using Cassette.BundleProcessing;
using Cassette.Stylesheets;
using Moq;
using Xunit;

namespace Cassette.Spriting.UnitTests
{
    public class StylesheetBundleExtensions
    {
        [Fact]
        public void SpriteImagesAddsSpriteImagesBundleProcessorToEndOfPipeline()
        {
            var pipeline = new Mock<IBundlePipeline<StylesheetBundle>>();
            const int endOfPipeline = 42;
            pipeline.SetupGet(p => p.Count).Returns(endOfPipeline);
            var bundle = new StylesheetBundle("~")
            {
                Pipeline = pipeline.Object
            };

            bundle.SpriteImages();

            pipeline.Verify(p => p.Insert<SpriteImages>(endOfPipeline));
        }
    }
}