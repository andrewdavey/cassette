using System;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class ConvertFontUrlsToDataUris : AddTransformerToAssets<StylesheetBundle>
    {
        readonly Func<string, bool> shouldEmbedUrl;

        public ConvertFontUrlsToDataUris()
        {
            shouldEmbedUrl = anyUrl => true;
        }

        public ConvertFontUrlsToDataUris(Func<string, bool> shouldEmbedUrl)
        {
            this.shouldEmbedUrl = shouldEmbedUrl;
        }

        protected override IAssetTransformer CreateAssetTransformer(StylesheetBundle bundle, CassetteSettings settings)
        {
            return new CssFontToDataUriTransformer(shouldEmbedUrl, settings.SourceDirectory);
        }
    }
}