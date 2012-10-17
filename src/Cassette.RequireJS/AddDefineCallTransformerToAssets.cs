using System;
using Cassette.BundleProcessing;
using Cassette.Scripts;

namespace Cassette.RequireJS
{
    public class AddDefineCallTransformerToAssets : IBundleProcessor<ScriptBundle>
    {
        readonly Func<DefineCallTransformer> create;

        public AddDefineCallTransformerToAssets(Func<DefineCallTransformer> create)
        {
            this.create = create;
        }

        public void Process(ScriptBundle bundle)
        {
            if (bundle.Path != "~/Cassette.RequireJS")
            {
                var assetTransformer = create();
                foreach (var asset in bundle.Assets)
                {
                    asset.AddAssetTransformer(assetTransformer);
                }   
            }
        }
    }
}