using System;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class CompileSass : IBundleProcessor<StylesheetBundle>
    {
        readonly ICompiler sassCompiler;

        public CompileSass(ICompiler sassCompiler)
        {
            this.sassCompiler = sassCompiler;
        }

        public void Process(StylesheetBundle bundle, CassetteSettings settings)
        {
            var sassAssets = bundle.Assets.Where(IsSassOrScss);
            foreach (var asset in sassAssets)
            {
                asset.AddAssetTransformer(new CompileAsset(sassCompiler));
            }
        }

        bool IsSassOrScss(IAsset asset)
        {
            var path = asset.SourceFile.FullPath;
            return path.EndsWith(".scss", StringComparison.OrdinalIgnoreCase) ||
                   path.EndsWith(".sass", StringComparison.OrdinalIgnoreCase);
        }
    }
}