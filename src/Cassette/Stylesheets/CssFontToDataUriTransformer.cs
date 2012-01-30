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
    class CssFontToDataUriTransformer : IAssetTransformer
    {
        public Func<string, bool> WhitelistFunc { get; set; }
        
        static readonly Regex UrlRegex = new Regex(
            @"\b url \s* \( \s* (?<quote>[""']?) (?<path>.*/embed/.*?)\.(?<extension>ttf|otf) \<quote> \s* \)",
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
                                      .Where(match => WhitelistFunc(match.Url))
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
                path = match.Groups["path"].Value;
                url = (path.StartsWith("/") ? "~" : "") + path + "." + match.Groups["extension"].Value;
                extension = match.Groups["extension"].Value;
                file = sourceAsset.SourceFile.Directory.GetFile(url);
            }

            readonly IAsset sourceAsset;
            readonly int index;
            readonly int length;
            readonly string path;
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
