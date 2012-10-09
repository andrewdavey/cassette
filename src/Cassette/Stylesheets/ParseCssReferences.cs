using System;
using Cassette.BundleProcessing;

namespace Cassette.Stylesheets
{
    public class ParseCssReferences : ParseReferences<StylesheetBundle>
    {
        protected override bool ShouldParseAsset(IAsset asset)
        {
            return asset.Path.EndsWith(".css", StringComparison.OrdinalIgnoreCase);
        }

        protected override ICommentParser CreateCommentParser()
        {
            return new CssCommentParser();
        }
    }
}