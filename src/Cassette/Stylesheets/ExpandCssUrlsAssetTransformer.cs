#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

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
    public class ExpandCssUrlsAssetTransformer : IAssetTransformer
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
            return asset.SourceFile.Directory.FullPath;
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
                .OrderByDescending(match => match.Index)
                .ToArray();
        }

        void ExpandUrl(StringBuilder builder, Group matchedUrlGroup, string relativeFilename)
        {
            relativeFilename = RemoveFragment(relativeFilename);
            var file = sourceDirectory.GetFile(relativeFilename.Substring(2));
            if (!file.Exists) return;

            var hash = HashFileContents(file);
            var absoluteUrl = urlGenerator.CreateRawFileUrl(relativeFilename, hash);
            builder.Remove(matchedUrlGroup.Index, matchedUrlGroup.Length);
            builder.Insert(matchedUrlGroup.Index, absoluteUrl);
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
            return PathUtilities.NormalizePath(PathUtilities.CombineWithForwardSlashes(currentDirectory, originalUrl));
        }
    }
}

