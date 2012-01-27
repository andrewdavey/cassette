using System;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class ConvertImageUrlsToDataUris : AddTransformerToAssets
    {
        public ConvertImageUrlsToDataUris(Func<string, bool> whitelistFunc)
            : base(new CssImageToDataUriTransformer() { WhitelistFunc = whitelistFunc })
        {
        }
    }
}
