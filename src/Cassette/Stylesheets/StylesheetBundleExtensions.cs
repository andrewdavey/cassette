using System;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public static class StylesheetBundleExtensions
    {
        public static StylesheetBundle EmbedImages(this StylesheetBundle bundle)
        {
            return bundle.EmbedImages(url => true);
        }

        public static StylesheetBundle EmbedImages(this StylesheetBundle bundle, Func<string, bool> shouldEmbedUrl)
        {
            var pipeline = bundle.Pipeline;
            var index = pipeline.IndexOf<ExpandCssUrls>();
            pipeline.Insert<ConvertImageUrlsToDataUris.Factory>(
                index,
                factory => factory(shouldEmbedUrl)
            );
            return bundle;
        }

        public static StylesheetBundle EmbedFonts(this StylesheetBundle bundle)
        {
            return bundle.EmbedFonts(path => true);
        }

        public static StylesheetBundle EmbedFonts(this StylesheetBundle bundle, Func<string, bool> shouldEmbedUrl)
        {
            var pipeline = bundle.Pipeline;
            var index = pipeline.IndexOf<ExpandCssUrls>();
            pipeline.Insert<ConvertFontUrlsToDataUris.Factory>(
                index,
                factory => factory(shouldEmbedUrl)
            );
            return bundle;
        }
    }
}