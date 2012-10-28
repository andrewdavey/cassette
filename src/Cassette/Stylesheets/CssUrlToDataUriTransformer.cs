using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    abstract class CssUrlToDataUriTransformer : IAssetTransformer
    {
        readonly Func<string, bool> shouldEmbedUrl;
        readonly Regex urlRegex;
        readonly IDirectory rootDirectory;

        protected CssUrlToDataUriTransformer(Func<string, bool> shouldEmbedUrl, Regex urlRegex, IDirectory rootDirectory)
        {
            if (shouldEmbedUrl == null)
            {
                throw new ArgumentNullException("shouldEmbedUrl");
            }
            if (urlRegex == null)
            {
                throw new ArgumentNullException("urlRegex");
            }
            if (rootDirectory == null)
            {
                throw new ArgumentNullException("rootDirectory");
            }

            this.shouldEmbedUrl = shouldEmbedUrl;
            this.urlRegex = urlRegex;
            this.rootDirectory = rootDirectory;
        }

        public string Transform(string css, IAsset asset)
        {
            var matches = urlRegex
                .Matches(css)
                .Cast<Match>()
                .Select(match => CreateCssUrlMatchTransformer(match, asset, rootDirectory))
                .Where(match => match.CanTransform && shouldEmbedUrl(match.Url))
                .Reverse(); // Must work backwards to prevent match indicies getting out of sync after insertions.

            var output = new StringBuilder(css);
            foreach (var match in matches)
            {
                match.Transform(output);

                asset.AddRawFileReference(match.File.FullPath);
            }
            return output.ToString();
        }

        protected abstract CssUrlMatchTransformer CreateCssUrlMatchTransformer(Match match, IAsset asset, IDirectory rootDirectory);
    }
}