using System.Collections.Generic;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetPipelineExtensions_Tests
    {
        readonly TestableStylesheetPipeline pipeline;

        public StylesheetPipelineExtensions_Tests()
        {
            pipeline = new TestableStylesheetPipeline();
        }

        [Fact]
        public void WhenEmbedImages_ThenPipelineContainsConvertImageUrlsToDataUris()
        {
            pipeline.EmbedImages();
            AssertPipelineContains<ConvertImageUrlsToDataUris>();
            AssertImageEmbedFormat(ImageEmbedType.DataUriForIE8);
        }

        [Fact]
        public void WhenEmbedImagesWithUrlPredicate_ThenPipelineContainsConvertImageUrlsToDataUris()
        {
            pipeline.EmbedImages(url => true);
            AssertPipelineContains<ConvertImageUrlsToDataUris>();
            AssertImageEmbedFormat(ImageEmbedType.DataUriForIE8);
        }

        [Fact]
        public void WhenEmbedImagesWithoutIE8Support_ThenPipelineContainsConvertImageUrlsToDataUris()
        {
            pipeline.EmbedImages(ImageEmbedType.DataUri);
            AssertPipelineContains<ConvertImageUrlsToDataUris>();
            AssertImageEmbedFormat(ImageEmbedType.DataUri);
        }

        [Fact]
        public void WhenEmbedImagesWithoutIE8SupportWithUrlPredicate_ThenPipelineContainsConvertImageUrlsToDataUris()
        {
            pipeline.EmbedImages(url => true, ImageEmbedType.DataUri);
            AssertPipelineContains<ConvertImageUrlsToDataUris>();
            AssertImageEmbedFormat(ImageEmbedType.DataUri);
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

        void AssertPipelineContains<T>() where T : IBundleProcessor<Bundle>
        {
            // MutablePipeline steps are actually created when Process is called.
            var bundle = new StylesheetBundle("~");
            pipeline.Process(bundle, new CassetteSettings(""));

            pipeline.CreatedPipeline.OfType<T>().ShouldNotBeEmpty();
        }

        private void AssertImageEmbedFormat(ImageEmbedType imageEmbedType)
        {
            var converter = pipeline.CreatedPipeline.OfType<ConvertImageUrlsToDataUris>().Single();

            if (imageEmbedType == ImageEmbedType.DataUri)
                Assert.False(converter.UseIE8Truncation);

            if (imageEmbedType == ImageEmbedType.DataUriForIE8)
                Assert.True(converter.UseIE8Truncation);
        }

        class TestableStylesheetPipeline : StylesheetPipeline
        {
            public IEnumerable<IBundleProcessor<StylesheetBundle>> CreatedPipeline { get; private set; }

            protected override IEnumerable<IBundleProcessor<StylesheetBundle>> CreateMutatedPipeline(StylesheetBundle bundle, CassetteSettings settings)
            {
                return CreatedPipeline = base.CreateMutatedPipeline(bundle, settings);
            }
        }
    }
}
