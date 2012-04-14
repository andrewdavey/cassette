using System;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class CompileLess : IBundleProcessor<StylesheetBundle>
    {
        readonly ICompiler lessCompiler;
        readonly CassetteSettings settings;

        public CompileLess(ICompiler lessCompiler, CassetteSettings settings)
        {
            this.lessCompiler = lessCompiler;
            this.settings = settings;
        }

        public void Process(StylesheetBundle bundle)
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