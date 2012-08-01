using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    class ExpandCssUrlsAssetTransformer : IAssetTransformer
    {
        readonly IDirectory sourceDirectory;
        readonly IUrlGenerator urlGenerator;

        public ExpandCssUrlsAssetTransformer(IDirectory sourceDirectory, IUrlGenerator urlGenerator)
        {
            this.sourceDirectory = sourceDirectory;
            this.urlGenerator = urlGenerator;
        }

        static readonly Regex CssUrlRegex = new Regex(
            @"\b url \s* \( \s* (?<quote>['""]?) (?<url>.*?) \<quote> \s* \)",
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
                    if (ReplaceUrlWithCassetteRawFileUrl(builder, matchedUrlGroup, relativeFilename))
                    {
                        asset.AddRawFileReference(relativeFilename);
                    }
                    else
                    {
                        ReplaceUrlWithAbsoluteUrl(builder, matchedUrlGroup, currentDirectory);
                    }
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
            var file = sourceDirectory.GetFile(asset.Path);
            return file.Directory.FullPath;
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
                .Where(IsRelativeUrl)
                .OrderByDescending(match => match.Index)
                .ToArray();
        }

        bool IsRelativeUrl(Match match)
        {
            return !AbsoluteUrlRegex.IsMatch(match.Groups["url"].Value);
        }

        bool ReplaceUrlWithCassetteRawFileUrl(StringBuilder builder, Group matchedUrlGroup, string filename)
        {
            filename = RemoveFragment(filename);
            var file = sourceDirectory.GetFile(filename);
            if (!file.Exists)
            {
                return false;
            }

            var hash = HashFileContents(file);
            var absoluteUrl = urlGenerator.CreateRawFileUrl(filename, hash);
            builder.Remove(matchedUrlGroup.Index, matchedUrlGroup.Length);
            builder.Insert(matchedUrlGroup.Index, absoluteUrl);
            return true;
        }

        string RemoveFragment(string relativeFilename)
        {
            var index = relativeFilename.IndexOf('#');
            if (index < 0) return relativeFilename;
            return relativeFilename.Substring(0, index);
        }

        string HashFileContents(IFile file)
        {
            using (var fileStream = file.OpenRead())
            {
                return fileStream.ComputeSHA1Hash().ToHexString();
            }
        }

        string GetImageFilename(Group matchedUrlGroup, string currentDirectory)
        {
            var originalUrl = matchedUrlGroup.Value.Trim('"', '\'');
            if (originalUrl.StartsWith("/"))
            {
                return PathUtilities.NormalizePath("~" + originalUrl);
            }
            return PathUtilities.NormalizePath(PathUtilities.CombineWithForwardSlashes(currentDirectory, originalUrl));
        }

        void ReplaceUrlWithAbsoluteUrl(StringBuilder builder, Group matchedUrlGroup, string currentDirectory)
        {
            var url = matchedUrlGroup.Value;

            // URLs that start with a "/" are assumed to be rooted, not relative to the virtual directory.
            // So leave them as they are.
            if (url.StartsWith("/")) return;

            var absoluteUrl = urlGenerator.CreateAbsolutePathUrl(currentDirectory + "/" + url);
            builder.Remove(matchedUrlGroup.Index, matchedUrlGroup.Length);
            builder.Insert(matchedUrlGroup.Index, absoluteUrl);
        }
    }
}