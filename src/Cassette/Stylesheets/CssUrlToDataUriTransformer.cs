using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    abstract class CssUrlToDataUriTransformer : IAssetTransformer
    {
        readonly Func<string, bool> shouldEmbedUrl;
        readonly Regex urlRegex;

        protected CssUrlToDataUriTransformer(Func<string, bool> shouldEmbedUrl, Regex urlRegex)
        {
            if (shouldEmbedUrl == null)
            {
                throw new ArgumentNullException("shouldEmbedUrl");
            }
            if (urlRegex == null)
            {
                throw new ArgumentNullException("urlRegex");
            }

            this.shouldEmbedUrl = shouldEmbedUrl;
            this.urlRegex = urlRegex;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var css = openSourceStream().ReadToEnd();
                var matches = urlRegex
                    .Matches(css)
                    .Cast<Match>()
                    .Select(match => CreateCssUrlMatchTransformer(match, asset))
                    .Where(match => match.CanTransform && shouldEmbedUrl(match.Url))
                    .Reverse(); // Must work backwards to prevent match indicies getting out of sync after insertions.

                var output = new StringBuilder(css);
                foreach (var match in matches)
                {
                    match.Transform(output);

                    asset.AddRawFileReference(match.Url);
                }
                return output.ToString().AsStream();
            };
        }

        protected abstract CssUrlMatchTransformer CreateCssUrlMatchTransformer(Match match, IAsset asset);
    }
}