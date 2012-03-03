using System;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class ConvertImageUrlsToDataUris : AddTransformerToAssets
    {
        public ConvertImageUrlsToDataUris(bool supportIE8)
            : base(new CssImageToDataUriTransformer(anyUrl => true, supportIE8))
        {   
        }

        public ConvertImageUrlsToDataUris(Func<string, bool> shouldEmbedUrl, bool supportIE8)
            : base(new CssImageToDataUriTransformer(shouldEmbedUrl, supportIE8))
        {
        }

        public bool UseIE8Truncation
        {
            get
            {
                return ((CssImageToDataUriTransformer)assetTransformer).SupportIE8;
            }
        }
    }
}