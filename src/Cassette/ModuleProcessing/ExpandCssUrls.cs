namespace Cassette.ModuleProcessing
{
    public class ExpandCssUrls : IModuleProcessor<StylesheetModule>
    {
        public void Process(StylesheetModule module, ICassetteApplication application)
        {
            foreach (var asset in module.Assets)
            {
                asset.AddAssetTransformer(new ExpandCssUrlsAssetTransformer(module, application));
            }
        }
    }
}