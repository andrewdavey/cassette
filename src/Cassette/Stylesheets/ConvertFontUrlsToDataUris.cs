using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class ConvertFontUrlsToDataUris : AddTransformerToAssets
    {
        public ConvertFontUrlsToDataUris()
            : base(new CssFontToDataUriTransformer())
        {
        }
    }
}
