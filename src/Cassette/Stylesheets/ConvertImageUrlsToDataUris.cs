using System;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class ConvertImageUrlsToDataUris : AddTransformerToAssets<StylesheetBundle>
    {
        readonly Func<string, bool> shouldEmbedUrl;

        public ConvertImageUrlsToDataUris(Func<string, bool> shouldEmbedUrl)
        {
            this.shouldEmbedUrl = shouldEmbedUrl;
        }

        protected override IAssetTransformer CreateAssetTransformer(StylesheetBundle bundle, CassetteSettings settings)
        {
            return new CssImageToDataUriTransformer(shouldEmbedUrl, settings.SourceDirectory);
        }
    }
}