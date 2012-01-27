using System;
using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public static class StylesheetPipelineExtensions {
        
        public static StylesheetPipeline EmbedImages(this StylesheetPipeline pipeline, Func<string, bool> whitelistFunc = null) {
            
            whitelistFunc = whitelistFunc ?? (whitelistPath => true);
            
            return (StylesheetPipeline)pipeline.InsertAfter<CompileLess>(new ConvertImageUrlsToDataUris(whitelistFunc));
        }
        
        public static StylesheetPipeline EmbedFonts(this StylesheetPipeline pipeline, Func<string, bool> whitelistFunc = null) {
            
            whitelistFunc = whitelistFunc ?? (whitelistPath => true);
            
            return (StylesheetPipeline)pipeline.InsertAfter<CompileLess>(new ConvertFontUrlsToDataUris(whitelistFunc));
        }
        
    }
}
