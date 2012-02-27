using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.IO;
using Cassette.Utilities;
using RequestReduce.Reducer;

namespace Cassette.Stylesheets
{
    class SpriteTransformer : IAssetTransformer
    {
        readonly ICssImageTransformer cssImageTransformer;
        readonly IDirectory sourceDirectory;
        readonly IUrlGenerator urlGenerator;

        public SpriteTransformer(IDirectory sourceDirectory, IUrlGenerator urlGenerator)
        {
            this.sourceDirectory = sourceDirectory;
            this.urlGenerator = urlGenerator;
            cssImageTransformer = new CssImageTransformer(new CssSelectorAnalyzer());
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

                foreach (var backgroundImageClass in cssImageTransformer.ExtractImageUrls(css))
                {
                    
                }

                return openSourceStream();
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
    }
}

