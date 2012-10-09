using Cassette.BundleProcessing;
using Cassette.TinyIoC;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplatePipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public HtmlTemplatePipeline(TinyIoCContainer container)
            : base(container)
        {
            AddRange(new IBundleProcessor<HtmlTemplateBundle>[]
            {
                new AssignHtmlTemplateRenderer(new InlineHtmlTemplateBundleRenderer()),
                new ParseHtmlTemplateReferences(),
                container.Resolve<WrapHtmlTemplatesInScriptElements>(),
                new AssignHash(),
                new ConcatenateAssets()
            });
        }
    }
}