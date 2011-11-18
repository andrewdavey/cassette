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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    class CssImageToDataUriTransformer : IAssetTransformer
    {
        static readonly Regex UrlRegex = new Regex(
            @"\b url \s* \( \s* (?<quote>[""']?) (?<path>.*?)\.(?<extension>png|jpg|jpeg|gif) \<quote> \s* \)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var css = openSourceStream().ReadToEnd();
                var matches = UrlRegex.Matches(css)
                                      .Cast<Match>()
                                      .Select(match => new UrlMatch(asset, match))
                                      .Reverse(); // Must work backwards to prevent match indicies getting out of sync after insertions.

                var output = new StringBuilder(css);
                foreach (var match in matches)
                {
                    match.ReplaceWithin(output);

                    asset.AddRawFileReference(match.Url);
                }
                return output.ToString().AsStream();
            };
        }

        class UrlMatch
        {
            public UrlMatch(IAsset asset, Match match)
            {
                sourceAsset = asset;
                index = match.Index; 
                length = match.Length;
                url = match.Groups["path"].Value + "." + match.Groups["extension"].Value;
                extension = match.Groups["extension"].Value;
                file = sourceAsset.SourceFile.Directory.GetFile(url);
            }

            readonly IAsset sourceAsset;
            readonly int index;
            readonly int length;
            readonly string url;
            readonly string extension;
            readonly IFile file;

            public string Url
            {
                get { return url; }
            }

            string DataUri
            {
                get
                {
                    return string.Format("url(data:{0};base64,{1})",
                        ContentType,
                        GetBase64EncodedData()
                    );
                }
            }

            string ContentType
            {
                get
                {
                    if (extension.Equals("jpg", StringComparison.OrdinalIgnoreCase))
                    {
                        return "image/jpeg";
                    }
                    else
                    {
                        return "image/" + extension.ToLowerInvariant();
                    }
                }
            }

            string GetBase64EncodedData()
            {
                using (var fileStream = file.OpenRead())
                using (var temp = new MemoryStream())
                using (var base64Stream = new CryptoStream(temp, new ToBase64Transform(), CryptoStreamMode.Write))
                {
                    fileStream.CopyTo(base64Stream);
                    base64Stream.Flush();
                    temp.Position = 0;
                    var reader = new StreamReader(temp);
                    return reader.ReadToEnd();
                }
            }

            public void ReplaceWithin(StringBuilder output)
            {
                if (!file.Exists) return;
                
                output.Remove(index, length);
                output.Insert(index, DataUri);
            }
        }
    }
}
