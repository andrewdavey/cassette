using Cassette.BundleProcessing;
using TinyIoC;

namespace Cassette.HtmlTemplates
{
    public class KnockoutJQueryTmplPipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public KnockoutJQueryTmplPipeline(TinyIoCContainer container)
            : base(container)
        {
            var renderer = container.Resolve<RemoteHtmlTemplateBundleRenderer>();
            AddRange(new IBundleProcessor<HtmlTemplateBundle>[]
            {
                container.Resolve<AssignHtmlTemplateRenderer.Factory>()(renderer),
                new AssignContentType("text/javascript"),
                new ParseHtmlTemplateReferences(),
                container.Resolve<CompileKnockoutJQueryTmpl>(),
                container.Resolve<RegisterTemplatesWithJQueryTmpl>(),
                new AssignHash(),
                new ConcatenateAssets()
            });
        }
    }
}