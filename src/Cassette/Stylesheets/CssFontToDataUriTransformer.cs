using System;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.IO;

namespace Cassette.Stylesheets
{
    class CssFontToDataUriTransformer : CssUrlToDataUriTransformer
    {
        static readonly Regex UrlRegex = new Regex(
            @"\b url \s* \( \s* (?<quote>[""']?) (?<path>.*?)\.(?<extension>ttf|otf) \<quote> \s* \)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );

        public CssFontToDataUriTransformer(Func<string, bool> shouldEmbedUrl, IDirectory rootDirectory)
            : base(shouldEmbedUrl, UrlRegex, rootDirectory)
        {
        }

        protected override CssUrlMatchTransformer CreateCssUrlMatchTransformer(Match match, IAsset asset, IDirectory rootDirectory)
        {
            return new CssFontUrlMatchTransformer(match, asset, rootDirectory);
        }

        class CssFontUrlMatchTransformer : CssUrlMatchTransformer
        {
            public CssFontUrlMatchTransformer(Match match, IAsset asset, IDirectory rootDirectory)
                : base(match, asset, rootDirectory)
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