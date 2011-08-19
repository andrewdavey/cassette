using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.ModuleProcessing;
using Cassette.Utilities;
using System.Security.Cryptography;

namespace Cassette.Stylesheets
{
    public class ExpandCssUrlsAssetTransformer : IAssetTransformer
    {
        public ExpandCssUrlsAssetTransformer(Module module, ICassetteApplication application)
        {
            this.module = module;
            this.application = application;
        }

        readonly Module module;
        readonly ICassetteApplication application;

        static readonly Regex cssUrlRegex = new Regex(
            @"\b url \s* \( \s* (?<url>.*?) \s* \)", 
            RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
        );
        static readonly Regex absoluteUrlRegex = new Regex(
            @"^(https?:|data:|//)"
        );

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var css = ReadCss(openSourceStream);
                var currentDirectory = GetCurrentDirectory(asset);
                var urlMatches = UrlMatchesInReverse(css);
                var builder = new StringBuilder(css);
                foreach (var match in urlMatches)
                {
                    var matchedUrlGroup = match.Groups["url"];
                    ExpandUrl(builder, matchedUrlGroup, currentDirectory);
                }
                return builder.ToString().AsStream();
            };
        }

        string ReadCss(Func<Stream> openSourceStream)
        {
            using (var reader = new StreamReader(openSourceStream()))
            {
                return reader.ReadToEnd();
            }
        }

        string GetCurrentDirectory(IAsset asset)
        {
            return Path.Combine(module.Directory, Path.GetDirectoryName(asset.SourceFilename));
        }

        /// <remarks>
        /// Matches need to be in reverse because we'll be modifying the string.
        /// Working backwards means we won't disturb the match index values.
        /// </remarks>
        IEnumerable<Match> UrlMatchesInReverse(string css)
        {
            return cssUrlRegex
                .Matches(css)
                .Cast<Match>()
                .Where(match => absoluteUrlRegex.IsMatch(match.Groups["url"].Value) == false)
                .OrderByDescending(match => match.Index);
        }

        void ExpandUrl(StringBuilder builder, Group matchedUrlGroup, string currentDirectory)
        {
            var originalUrl = matchedUrlGroup.Value.Trim('"', '\'');
            var relativeUrl = PathUtilities.NormalizePath(Path.Combine(currentDirectory, originalUrl));

            string hash;
            using (var file = application.RootDirectory.OpenFile(relativeUrl, FileMode.Open, FileAccess.Read))
            {
                hash = file.ComputeSHA1Hash().ToHexString();
            }

            var absoluteUrl = application.UrlGenerator.CreateImageUrl(relativeUrl, hash);
            builder.Remove(matchedUrlGroup.Index, matchedUrlGroup.Length);
            builder.Insert(matchedUrlGroup.Index, absoluteUrl);
        }
    }
}
