using System.Linq;
using Cassette.BundleProcessing;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetPipelineExtensions_Tests
    {
        readonly StylesheetPipeline pipeline;

        public StylesheetPipelineExtensions_Tests()
        {
            pipeline = new StylesheetPipeline();
        }

        [Fact]
        public void WhenEmbedImages_ThenPipelineContainsConvertImageUrlsToDataUris()
        {
            pipeline.EmbedImages();
            AssertPipelineContains<ConvertImageUrlsToDataUris>();
        }

        [Fact]
        public void WhenEmbedImagesWithUrlPredicate_ThenPipelineContainsConvertImageUrlsToDataUris()
        {
            pipeline.EmbedImages(url => true);
            AssertPipelineContains<ConvertImageUrlsToDataUris>();
        }
        
        [Fact]
        public void WhenEmbedFonts_ThenPipelineContainsConvertFontUrlsToDataUris()
        {
            pipeline.EmbedFonts();
            AssertPipelineContains<ConvertFontUrlsToDataUris>();
        }

        [Fact]
        public void WhenEmbedFontsWithUrlPredicate_ThenPipelineContainsConvertFontUrlsToDataUris()
        {
            pipeline.EmbedFonts(url => true);
            AssertPipelineContains<ConvertFontUrlsToDataUris>();
        }

        void AssertPipelineContains<T>() where T : IBundleProcessor<StylesheetBundle>
        {
            pipeline.OfType<T>().ShouldNotBeEmpty();
        }
    }
}
