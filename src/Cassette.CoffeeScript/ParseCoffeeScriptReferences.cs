using System;
using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public class ParseCoffeeScriptReferences : ParseReferences<ScriptBundle>
    {
        protected override bool ShouldParseAsset(IAsset asset)
        {
            return asset.Path.EndsWith(".coffee", StringComparison.OrdinalIgnoreCase);
        }

        internal override ICommentParser CreateCommentParser()
        {
            return new CoffeeScriptCommentParser();
        }
    }
}