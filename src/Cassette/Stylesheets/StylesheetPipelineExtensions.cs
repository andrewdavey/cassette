using System;

namespace Cassette.Stylesheets
{
    public static class StylesheetPipelineExtensions
    {
        public static StylesheetPipeline EmbedImages(this StylesheetPipeline pipeline, ImageEmbedType type = ImageEmbedType.DataUriForIE8)
        {
            return pipeline.EmbedImages(url => true, type);
        }

        public static StylesheetPipeline EmbedImages(this StylesheetPipeline pipeline, Func<string, bool> shouldEmbedUrl, ImageEmbedType type = ImageEmbedType.DataUriForIE8)
        {
            if (type == ImageEmbedType.Mhtml)
            {
                // TODO: MHTML support
            }
            else
            {
                bool ie8Support = (type == ImageEmbedType.DataUriForIE8);
                pipeline.InsertBefore<ExpandCssUrls>(new ConvertImageUrlsToDataUris(shouldEmbedUrl, ie8Support));
            }
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