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
            // MutablePipeline steps are actually created when Process is called.
            var bundle = new StylesheetBundle("~");
            pipeline.Process(bundle, new CassetteSettings("") { SourceDirectory = new FakeFileSystem() });

            pipeline.CreatedPipeline.OfType<T>().ShouldNotBeEmpty();
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
