using System;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class ConvertFontUrlsToDataUris : AddTransformerToAssets
    {
        public ConvertFontUrlsToDataUris(Func<string, bool> whitelistFunc = null)
            : base(new CssFontToDataUriTransformer(){ WhitelistFunc = whitelistFunc })
        {
        }
    }
}
