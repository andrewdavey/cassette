using System;
using Cassette.BundleProcessing;
using Cassette.Scripts;

namespace Cassette.RequireJS
{
    public class AddDefineCallTransformerToAssets : AddTransformerToAssets<ScriptBundle>
    {
        readonly Func<DefineCallTransformer> create;

        public AddDefineCallTransformerToAssets(Func<DefineCallTransformer> create)
        {
            this.create = create;
        }

        protected override IAssetTransformer CreateAssetTransformer(ScriptBundle bundle)
        {
            return create();
        }
    }
}