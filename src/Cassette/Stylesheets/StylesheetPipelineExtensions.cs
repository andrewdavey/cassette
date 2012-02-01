using System;

namespace Cassette.Stylesheets
{
    public static class StylesheetPipelineExtensions
    {
        public static StylesheetPipeline EmbedImages(this StylesheetPipeline pipeline)
        {
            pipeline.InsertBefore<ExpandCssUrls>(new ConvertImageUrlsToDataUris());
            return pipeline;
        }

        public static StylesheetPipeline EmbedImages(this StylesheetPipeline pipeline, Func<string, bool> shouldEmbedUrl)
        {
            pipeline.InsertBefore<ExpandCssUrls>(new ConvertImageUrlsToDataUris(shouldEmbedUrl));
            return pipeline;
        }

        public static StylesheetPipeline EmbedFonts(this StylesheetPipeline pipeline)
        {
            pipeline.InsertBefore<ExpandCssUrls>(new ConvertFontUrlsToDataUris());
            return pipeline;
        }

        public static StylesheetPipeline EmbedFonts(this StylesheetPipeline pipeline, Func<string, bool> shouldEmbedUrl)
        {
            pipeline.InsertBefore<ExpandCssUrls>(new ConvertFontUrlsToDataUris(shouldEmbedUrl));
            return pipeline;
        }
    }
}