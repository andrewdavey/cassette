using System;
using System.Text;
using System.Text.RegularExpressions;
using Cassette.IO;

namespace Cassette.Stylesheets
{
    class CssImageToDataUriTransformer : CssUrlToDataUriTransformer
    {
        static readonly Regex BackgroundUrlRegex = new Regex(
            @"\b(?<start>background .*? url \s* \() \s* (?<quote>[""']?) (?<path>.*?)\.(?<extension>png|jpg|jpeg|gif) \<quote> \s* (?<end>\) .*? ;)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
        );

        public CssImageToDataUriTransformer(Func<string, bool> shouldEmbedUrl, bool supportIE8)
            : base(shouldEmbedUrl, BackgroundUrlRegex)
        {
            SupportIE8 = supportIE8;
        }

        public bool SupportIE8 { get; set; }

        protected override CssUrlMatchTransformer CreateCssUrlMatchTransformer(Match match, IAsset asset)
        {
            return new CssBackgroundImageUrlMatchTransformer(match, asset, SupportIE8);
        }

        class CssBackgroundImageUrlMatchTransformer : CssUrlMatchTransformer
        {
            readonly string fullProperty;
            readonly string start;
            readonly string end;
            readonly bool useIE8SizeRestrictions;

            public CssBackgroundImageUrlMatchTransformer(Match match, IAsset asset, bool supportIE8)
                : base(match, asset)
            {
                fullProperty = match.Value;
                start = match.Groups["start"].Value;
                end = match.Groups["end"].Value;
                useIE8SizeRestrictions = supportIE8;
            }
            
            protected override string GetContentType(string extension)
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

            public override bool CanTransform
            {
                get
                {
                    // If we are restricting to IE8 size (32kb) enforce that, otherwise embed all sizes
                    return base.CanTransform && (!useIE8SizeRestrictions || (useIE8SizeRestrictions && !FileIsTooLargeForInternetExplorer8()));
                }
            }

            bool FileIsTooLargeForInternetExplorer8()
            {
                const int internetExplorer8DataUriSizeLimitInBytes = 32768;
                using (var fileStream = File.OpenRead())
                {
                    return fileStream.Length > internetExplorer8DataUriSizeLimitInBytes;
                }
            }
            
            public override void Transform(StringBuilder css)
            {
                InsertCopyOfPropertyWithUrlReplacedWithDataUri(css);
            }

            void InsertCopyOfPropertyWithUrlReplacedWithDataUri(StringBuilder css)
            {
                var newBackgroundImageProperty = start + DataUri + end;
                var afterFullProperty = MatchIndex + fullProperty.Length;
                css.Insert(afterFullProperty, newBackgroundImageProperty);
            }
        }
    }
}