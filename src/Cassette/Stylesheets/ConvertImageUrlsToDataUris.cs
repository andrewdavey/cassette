using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class ConvertImageUrlsToDataUris : AddTransformerToAssets
    {
        public ConvertImageUrlsToDataUris()
            : base(new CssImageToDataUriTransformer())
        {
        }
    }
}