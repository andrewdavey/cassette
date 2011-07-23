using System;

namespace Cassette.Assets.Scripts
{
    public class UnresolvedScriptModuleBuilder : UnresolvedModuleBuilder
    {
        public UnresolvedScriptModuleBuilder(string rootDirectory)
            : base(rootDirectory, new[] { "js", "coffee" })
        {
        }

        protected override bool ShouldNotIgnoreAsset(string filename)
        {
            return !filename.EndsWith("-vsdoc.js");
        }

        protected override IUnresolvedAssetParser CreateParser(string filename)
        {
            var isCoffeeScript = filename.EndsWith(".coffee", StringComparison.InvariantCulture);
            if (isCoffeeScript)
            {
                return new UnresolvedCoffeeScriptParser();
            }
            else
            {
                return new UnresolvedJavaScriptParser();
            }
        }
    }
}
