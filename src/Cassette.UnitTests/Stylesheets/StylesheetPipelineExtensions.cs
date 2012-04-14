using System.Linq;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetPipelineExtensions_Tests
    {
        readonly StylesheetPipeline pipeline;

        public StylesheetPipelineExtensions_Tests()
        {
            var container = new TinyIoCContainer();
            var settings = new CassetteSettings();
            container.Register(Mock.Of<IStylesheetMinifier>());
            container.Register(Mock.Of<IUrlGenerator>());
            container.Register(settings);
            pipeline = new StylesheetPipeline(container, settings);
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
