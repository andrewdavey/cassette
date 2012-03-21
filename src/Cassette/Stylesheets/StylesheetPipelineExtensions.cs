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
            pipeline.InsertBefore<ExpandCssUrls, StylesheetBundle>(new ConvertImageUrlsToDataUris(shouldEmbedUrl));
            return pipeline;
        }

        public static StylesheetPipeline EmbedFonts(this StylesheetPipeline pipeline)
        {
            pipeline.InsertBefore<ExpandCssUrls, StylesheetBundle>(new ConvertFontUrlsToDataUris());
            return pipeline;
        }

        public static StylesheetPipeline EmbedFonts(this StylesheetPipeline pipeline, Func<string, bool> shouldEmbedUrl)
        {
            pipeline.InsertBefore<ExpandCssUrls, StylesheetBundle>(new ConvertFontUrlsToDataUris(shouldEmbedUrl));
            return pipeline;
        }
    }
}