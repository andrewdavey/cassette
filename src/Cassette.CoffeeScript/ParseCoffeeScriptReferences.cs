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

        protected override ICommentParser CreateCommentParser()
        {
            return new CoffeeScriptCommentParser();
        }
    }
}