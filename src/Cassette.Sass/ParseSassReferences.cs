using System;
using Cassette.BundleProcessing;
using Cassette.Scripts;

namespace Cassette.Stylesheets
{
    public class ParseSassReferences : ParseReferences<StylesheetBundle>
    {
        protected override bool ShouldParseAsset(IAsset asset)
        {
            var path = asset.Path;
            return path.EndsWith(".scss", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".sass", StringComparison.OrdinalIgnoreCase);
        }

        internal override ICommentParser CreateCommentParser()
        {
            // Sass supports the same comment syntax as JavaScript.
            // So we'll just reuse the JavaScript comment parser!
            return new JavaScriptCommentParser();
        }
    }
}