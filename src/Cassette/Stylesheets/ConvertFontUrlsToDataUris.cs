using System;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class ConvertFontUrlsToDataUris : AddTransformerToAssets
    {
        public ConvertFontUrlsToDataUris()
            : base(new CssFontToDataUriTransformer(anyUrl => true))
        {
        }

        public ConvertFontUrlsToDataUris(Func<string, bool> shouldEmbedUrl)
            : base(new CssFontToDataUriTransformer(shouldEmbedUrl))
        {
        }
    }
}