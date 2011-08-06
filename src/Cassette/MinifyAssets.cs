namespace Cassette
{
    public class MinifyAssets : IModuleProcessor<Module>
    {
        public MinifyAssets(IAssetTransformer minifier)
        {
            this.minifier = minifier;
        }

        readonly IAssetTransformer minifier;

        public void Process(Module module)
        {
            foreach (var asset in module.Assets)
            {
                asset.AddAssetTransformer(minifier);
            }
        }
    }
}
