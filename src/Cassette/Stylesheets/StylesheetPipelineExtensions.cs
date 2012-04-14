using System;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public static class StylesheetPipelineExtensions
    {
        public static StylesheetPipeline EmbedImages(this StylesheetPipeline pipeline)
        {
            return pipeline.EmbedImages(url => true);
        }

        public static StylesheetPipeline EmbedImages(this StylesheetPipeline pipeline, Func<string, bool> shouldEmbedUrl)
        {
            var factory = pipeline.Container.Resolve<ConvertImageUrlsToDataUris.Factory>();
            var step = factory(shouldEmbedUrl);
            pipeline.InsertBefore<ExpandCssUrls, StylesheetBundle>(step);
            return pipeline;
        }

        public static StylesheetPipeline EmbedFonts(this StylesheetPipeline pipeline)
        {
            return pipeline.EmbedFonts(path => true);
        }

        public static StylesheetPipeline EmbedFonts(this StylesheetPipeline pipeline, Func<string, bool> shouldEmbedUrl)
        {
            var factory = pipeline.Container.Resolve<ConvertFontUrlsToDataUris.Factory>();
            var step = factory(shouldEmbedUrl);
            pipeline.InsertBefore<ExpandCssUrls, StylesheetBundle>(step);
            return pipeline;
        }
    }
}