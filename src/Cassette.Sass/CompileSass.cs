using System;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class CompileSass : IBundleProcessor<StylesheetBundle>
    {
        readonly ICompiler sassCompiler;
        readonly CassetteSettings settings;

        public CompileSass(ICompiler sassCompiler, CassetteSettings settings)
        {
            this.sassCompiler = sassCompiler;
            this.settings = settings;
        }

        public void Process(StylesheetBundle bundle)
        {
            var sassAssets = bundle.Assets.Where(IsSassOrScss);
            foreach (var asset in sassAssets)
            {
                asset.AddAssetTransformer(new CompileAsset(sassCompiler, settings.SourceDirectory));
            }
        }

        bool IsSassOrScss(IAsset asset)
        {
            var path = asset.Path;
            return path.EndsWith(".scss", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".sass", StringComparison.OrdinalIgnoreCase);
        }
    }
}