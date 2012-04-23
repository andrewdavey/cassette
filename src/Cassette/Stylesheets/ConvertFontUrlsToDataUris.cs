using System;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class ConvertFontUrlsToDataUris : AddTransformerToAssets<StylesheetBundle>
    {
        public delegate ConvertFontUrlsToDataUris Factory(Func<string, bool> shouldEmbedUrl);

        readonly Func<string, bool> shouldEmbedUrl;
        readonly CassetteSettings settings;

        public ConvertFontUrlsToDataUris(Func<string, bool> shouldEmbedUrl, CassetteSettings settings)
        {
            this.shouldEmbedUrl = shouldEmbedUrl;
            this.settings = settings;
        }

        protected override IAssetTransformer CreateAssetTransformer(StylesheetBundle bundle)
        {
            return new CssFontToDataUriTransformer(shouldEmbedUrl, settings.SourceDirectory);
        }
    }
}