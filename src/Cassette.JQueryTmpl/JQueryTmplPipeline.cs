using Cassette.BundleProcessing;
using Cassette.TinyIoC;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplPipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public JQueryTmplPipeline(TinyIoCContainer container)
            : base(container)
        {
            var renderer = container.Resolve<RemoteHtmlTemplateBundleRenderer>();
            AddRange(new IBundleProcessor<HtmlTemplateBundle>[]
            {
                new AssignHtmlTemplateRenderer(renderer),
                new AssignContentType("text/javascript"),
                new ParseHtmlTemplateReferences(),
                container.Resolve<CompileJQueryTmpl>(),
                container.Resolve<RegisterTemplatesWithJQueryTmpl>(),
                new AssignHash(),
                new ConcatenateAssets()
            });
        }
    }
}