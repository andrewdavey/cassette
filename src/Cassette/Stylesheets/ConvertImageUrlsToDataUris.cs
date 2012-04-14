using System;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class ConvertImageUrlsToDataUris : AddTransformerToAssets<StylesheetBundle>
    {
        public delegate ConvertImageUrlsToDataUris Factory(Func<string, bool> shouldEmbedUrl);

        readonly Func<string, bool> shouldEmbedUrl;
        readonly CassetteSettings settings;

        public ConvertImageUrlsToDataUris(Func<string, bool> shouldEmbedUrl, CassetteSettings settings)
        {
            this.shouldEmbedUrl = shouldEmbedUrl;
            this.settings = settings;
        }

        protected override IAssetTransformer CreateAssetTransformer(StylesheetBundle bundle)
        {
            return new CssImageToDataUriTransformer(shouldEmbedUrl, settings.SourceDirectory);
        }
    }
}