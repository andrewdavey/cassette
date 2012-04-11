using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplPipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public JQueryTmplPipeline(IUrlGenerator urlGenerator)
        {
            AddRange(new IBundleProcessor<HtmlTemplateBundle>[]
            {
                new AssignHtmlTemplateRenderer(new RemoteHtmlTemplateBundleRenderer(urlGenerator)),
                new AssignContentType("text/javascript"),
                new ParseHtmlTemplateReferences(),
                new CompileJQueryTmpl(),
                new RegisterTemplatesWithJQueryTmpl(),
                new AssignHash(),
                new ConcatenateAssets()
            });
        }
    }
}