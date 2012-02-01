using System;
using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public static class StylesheetPipelineExtensions {
        
        public static StylesheetPipeline EmbedImages(this StylesheetPipeline pipeline, Func<string, bool> whitelistFunc = null) {
            return (StylesheetPipeline)pipeline.InsertBefore<ExpandCssUrls>(new ConvertImageUrlsToDataUris(whitelistFunc));
        }
        
        public static StylesheetPipeline EmbedFonts(this StylesheetPipeline pipeline, Func<string, bool> whitelistFunc = null) {
            return (StylesheetPipeline)pipeline.InsertBefore<ExpandCssUrls>(new ConvertFontUrlsToDataUris(whitelistFunc));
        }
        
    }
}
