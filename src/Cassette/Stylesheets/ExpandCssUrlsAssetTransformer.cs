using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.Utilities;

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

        static readonly Regex CssUrlRegex = new Regex(
            @"\b url \s* \( \s* (?<url>.*?) \s* \)", 
            RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
        );
        static readonly Regex AbsoluteUrlRegex = new Regex(
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
                    var relativeFilename = GetImageFilename(matchedUrlGroup, currentDirectory);
                    ExpandUrl(builder, matchedUrlGroup, relativeFilename);

                    asset.AddRawFileReference(relativeFilename);
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
            return Path.Combine(module.Path, Path.GetDirectoryName(asset.SourceFilename));
        }

        /// <remarks>
        /// Matches need to be in reverse because we'll be modifying the string.
        /// Working backwards means we won't disturb the match index values.
        /// </remarks>
        IEnumerable<Match> UrlMatchesInReverse(string css)
        {
            return CssUrlRegex
                .Matches(css)
                .Cast<Match>()
                .Where(match => AbsoluteUrlRegex.IsMatch(match.Groups["url"].Value) == false)
                .OrderByDescending(match => match.Index);
        }

        void ExpandUrl(StringBuilder builder, Group matchedUrlGroup, string relativeFilename)
        {
            var hash = HashFileContents(relativeFilename);
            var absoluteUrl = application.UrlGenerator.CreateImageUrl(relativeFilename, hash);
            builder.Remove(matchedUrlGroup.Index, matchedUrlGroup.Length);
            builder.Insert(matchedUrlGroup.Index, absoluteUrl);
        }

        string HashFileContents(string relativeFilename)
        {
            using (var file = application.RootDirectory.OpenFile(relativeFilename, FileMode.Open, FileAccess.Read))
            {
                return file.ComputeSHA1Hash().ToHexString();
            }
        }

        string GetImageFilename(Group matchedUrlGroup, string currentDirectory)
        {
            var originalUrl = matchedUrlGroup.Value.Trim('"', '\'');
            var relativeUrl = PathUtilities.NormalizePath(Path.Combine(currentDirectory, originalUrl));
            return relativeUrl;
        }
    }
}
