using System;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class ConvertImageUrlsToDataUris : AddTransformerToAssets
    {
        public ConvertImageUrlsToDataUris()
            : base(new CssImageToDataUriTransformer(anyUrl => true))
        {   
        }

        public ConvertImageUrlsToDataUris(Func<string, bool> shouldEmbedUrl)
            : base(new CssImageToDataUriTransformer(shouldEmbedUrl))
        {
        }
    }
}