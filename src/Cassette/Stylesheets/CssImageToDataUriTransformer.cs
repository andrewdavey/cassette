﻿using System;
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
        public Func<string, bool> WhitelistFunc { get; set; }
        
        static readonly Regex UrlRegex = new Regex(
            @"\b url \s* \( \s* (?<quote>[""']?) (?<path>.*?)\.(?<extension>png|jpg|jpeg|gif) \<quote> \s* \)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );
        
        static readonly Regex BackgroundUrlRegex = new Regex(
            @"\bbackground .* (?<value>url \s* \( \s* (?<quote>[""']?) (?<path>.*?)\.(?<extension>png|jpg|jpeg|gif) \<quote> \s* \)?) .* ;",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );
        
        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                var css = openSourceStream().ReadToEnd();
                var matches = BackgroundUrlRegex.Matches(css)
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
                property = match.Value;
                valueIndex = match.Groups["value"].Index;
                valueLength = match.Groups["value"].Length;
                path = match.Groups["path"].Value;
                url = (path.StartsWith("/") ? "~" : "") + path + "." + match.Groups["extension"].Value;
                extension = match.Groups["extension"].Value;
                file = sourceAsset.SourceFile.Directory.GetFile(url);
            }
            
            readonly IAsset sourceAsset;
            readonly int index;
            readonly int length;
            readonly string property;
            readonly int valueIndex;
            readonly int valueLength;
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
                if (!file.Exists)
                {
                    return;
                }
                
                // Internet Explorer 8 will not render images larger than 32 kB
                if (file.Exists
                    && file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite).Length > 32000)
                {
                    return;
                }
                
                Trace.Source.TraceInformation(string.Format("Embedded image {0}", path));
                
                output.Remove(valueIndex, valueLength);
                output.Insert(valueIndex, DataUri);
                output.Insert(index, property + "\n");
            }
        }
    }
}
