using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplatePipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public HtmlTemplatePipeline()
        {
            AddRange(new IBundleProcessor<HtmlTemplateBundle>[]
            {
                new AssignHtmlTemplateRenderer(settings => new InlineHtmlTemplateBundleRenderer()),
                new ParseHtmlTemplateReferences(),
                new WrapHtmlTemplatesInScriptElements(),
                new AssignHash(),
                new ConcatenateAssets()
            });
        }
    }
}