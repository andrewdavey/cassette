using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Cassette.Stylesheets
{
    class CssFontToDataUriTransformer : CssUrlToDataUriTransformer
    {
        static readonly Regex UrlRegex = new Regex(
            @"\b url \s* \( \s* (?<quote>[""']?) (?<path>.*?)\.(?<extension>ttf|otf) \<quote> \s* \)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );

        public CssFontToDataUriTransformer(Func<string, bool> shouldEmbedUrl)
            : base(shouldEmbedUrl, UrlRegex)
        {
        }

        protected override CssUrlMatchTransformer CreateCssUrlMatchTransformer(Match match, IAsset asset)
        {
            return new CssFontUrlMatchTransformer(match, asset);
        }

        class CssFontUrlMatchTransformer : CssUrlMatchTransformer
        {
            public CssFontUrlMatchTransformer(Match match, IAsset asset)
                : base(match, asset)
            {
            }

            protected override string GetContentType(string extension)
            {
                return "font/" + extension;
            }

            public override void Transform(StringBuilder css)
            {
                css.Remove(MatchIndex, MatchLength);
                css.Insert(MatchIndex, "url(" + DataUri + ")");
            }
        }
    }
}