namespace Cassette.ModuleProcessing
{
    public class AddTransformerToAssets : IModuleProcessor<Module>
    {
        public AddTransformerToAssets(IAssetTransformer assetTransformer)
        {
            this.assetTransformer = assetTransformer;
        }

        readonly IAssetTransformer assetTransformer;

        public void Process(Module module, ICassetteApplication application)
        {
            foreach (var asset in module.Assets)
            {
                asset.AddAssetTransformer(assetTransformer);
            }
        }
    }
}