namespace Cassette.ModuleProcessing
{
    public class WrapHtmlTemplatesInScriptBlocks : IModuleProcessor<HtmlTemplateModule>
    {
        /// <summary>
        /// Wraps each HTML template in a &lt;script&gt; block with the type attribute "text/html".
        /// </summary>
        public WrapHtmlTemplatesInScriptBlocks()
            : this("text/html")
        {
        }

        /// <summary>
        /// Wraps each HTML template in a &lt;script&gt; block with the given type attribute.
        /// </summary>
        public WrapHtmlTemplatesInScriptBlocks(string contentType)
        {
            this.contentType = contentType;
        }

        readonly string contentType;

        public void Process(HtmlTemplateModule module)
        {
            foreach (var asset in module.Assets)
            {
                asset.AddAssetTransformer(new WrapHtmlTemplateInScriptBlock(contentType));
            }
        }
    }
}
