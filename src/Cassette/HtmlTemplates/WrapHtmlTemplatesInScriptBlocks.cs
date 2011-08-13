using Cassette.ModuleProcessing;

namespace Cassette.HtmlTemplates
{
    public class WrapHtmlTemplatesInScriptBlocks : IModuleProcessor<HtmlTemplateModule>
    {
        public void Process(HtmlTemplateModule module, ICassetteApplication application)
        {
            foreach (var asset in module.Assets)
            {
                asset.AddAssetTransformer(new WrapHtmlTemplateInScriptBlock(module));
            }
        }
    }
}
