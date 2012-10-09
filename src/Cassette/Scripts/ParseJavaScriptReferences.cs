using System;
using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public class ParseJavaScriptReferences : ParseReferences<ScriptBundle>
    {
        protected override bool ShouldParseAsset(IAsset asset)
        {
            return asset.Path.EndsWith(".js", StringComparison.OrdinalIgnoreCase);
        }

        protected override ICommentParser CreateCommentParser()
        {
            return new JavaScriptCommentParser();
        }

        internal override ReferenceParser CreateReferenceParser(ICommentParser commentParser)
        {
            return new JavaScriptReferenceParser(commentParser);
        }
    }
}