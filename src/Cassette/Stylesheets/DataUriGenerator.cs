﻿using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    public class DataUriGenerator : IAssetTransformer
    {
        static readonly Regex urlRegex = new Regex(
            @"\b url \s* \( \s* (?<quote>[""']?) (?<path>.*?)\.(?<extension>png|jpg|jpeg|gif) \<quote> \s* \)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var css = openSourceStream().ReadToEnd();
                var matches = urlRegex.Matches(css)
                                      .Cast<Match>()
                                      .Select(match => new UrlMatch(asset, match))
                                      .Reverse(); // Must work backwards to prevent match indicies getting out of sync after insertions.

                var output = new StringBuilder(css);
                foreach (var match in matches)
                {
                    match.ReplaceWithin(output);
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
            }

            readonly IAsset sourceAsset;
            readonly int index;
            readonly int length;
            readonly string url;
            readonly string extension;

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
                var dir = sourceAsset.Directory.NavigateTo(Path.GetDirectoryName(sourceAsset.SourceFilename), false);

                using (var file = dir.OpenFile(url, FileMode.Open, FileAccess.Read))
                using (var temp = new MemoryStream())
                using (var base64Stream = new CryptoStream(temp, new ToBase64Transform(), CryptoStreamMode.Write))
                {
                    file.CopyTo(base64Stream);
                    base64Stream.Flush();
                    temp.Position = 0;
                    var reader = new StreamReader(temp);
                    return reader.ReadToEnd();
                }
            }

            public void ReplaceWithin(StringBuilder output)
            {
                output.Remove(index, length);
                output.Insert(index, DataUri);
            }
        }
    }
}