using System;

namespace Cassette.Stylesheets
{
    public static class StylesheetPipelineExtensions
    {
        public static StylesheetPipeline EmbedImages(this StylesheetPipeline pipeline, Func<string, bool> whitelistFunc = null)
        {
            pipeline.InsertBefore<ExpandCssUrls>(new ConvertImageUrlsToDataUris(whitelistFunc));
            return pipeline;
        }
        
        public static StylesheetPipeline EmbedFonts(this StylesheetPipeline pipeline, Func<string, bool> whitelistFunc = null)
        {
            pipeline.InsertBefore<ExpandCssUrls>(new ConvertFontUrlsToDataUris(whitelistFunc));
            return pipeline;
        }
    }
}