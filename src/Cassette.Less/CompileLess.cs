using System;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class CompileLess : IBundleProcessor<StylesheetBundle>
    {
        readonly ICompiler lessCompiler;

        public CompileLess(ICompiler lessCompiler)
        {
            this.lessCompiler = lessCompiler;
        }

        public void Process(StylesheetBundle bundle, CassetteSettings settings)
        {
            foreach (var asset in bundle.Assets)
            {
                if (asset.Path.EndsWith(".less", StringComparison.OrdinalIgnoreCase))
                {
                    asset.AddAssetTransformer(new CompileAsset(lessCompiler, settings.SourceDirectory));
                }
            }
        }
    }
}