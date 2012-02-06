using System;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class OptimizeImages : IBundleProcessor<StylesheetBundle>
    {
        public void Process(StylesheetBundle bundle, CassetteSettings settings)
        {
            foreach (var asset in bundle.Assets)
            {
                asset.AddAssetTransformer(new OptimizeImageTransformer(settings));
            }
        }
    }
}