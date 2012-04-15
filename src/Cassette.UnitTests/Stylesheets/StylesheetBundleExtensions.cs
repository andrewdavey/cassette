using System.Linq;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using Should;
using TinyIoC;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetBundleExtensions_Tests
    {
        readonly StylesheetPipeline pipeline;
        readonly StylesheetBundle bundle;

        public StylesheetBundleExtensions_Tests()
        {
            var container = new TinyIoCContainer();
            var settings = new CassetteSettings();
            container.Register(Mock.Of<IStylesheetMinifier>());
            container.Register(Mock.Of<IUrlGenerator>());
            container.Register(settings);
            pipeline = new StylesheetPipeline(container, settings);
            bundle = new StylesheetBundle("~")
            {
                Pipeline = pipeline
            };
        }

        [Fact]
        public void WhenEmbedImages_ThenPipelineContainsConvertImageUrlsToDataUris()
        {
            bundle.EmbedImages();
            AssertPipelineContains<ConvertImageUrlsToDataUris>();
        }

        [Fact]
        public void WhenEmbedImagesWithUrlPredicate_ThenPipelineContainsConvertImageUrlsToDataUris()
        {
            bundle.EmbedImages(url => true);
            AssertPipelineContains<ConvertImageUrlsToDataUris>();
        }
        
        [Fact]
        public void WhenEmbedFonts_ThenPipelineContainsConvertFontUrlsToDataUris()
        {
            bundle.EmbedFonts();
            AssertPipelineContains<ConvertFontUrlsToDataUris>();
        }

        [Fact]
        public void WhenEmbedFontsWithUrlPredicate_ThenPipelineContainsConvertFontUrlsToDataUris()
        {
            bundle.EmbedFonts(url => true);
            AssertPipelineContains<ConvertFontUrlsToDataUris>();
        }

        void AssertPipelineContains<T>() where T : IBundleProcessor<StylesheetBundle>
        {
            pipeline.OfType<T>().ShouldNotBeEmpty();
        }
    }
}
