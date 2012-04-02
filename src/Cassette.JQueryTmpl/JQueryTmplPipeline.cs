using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplPipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public JQueryTmplPipeline(IUrlGenerator urlGenerator)
        {
            AddRange(new IBundleProcessor<HtmlTemplateBundle>[]
            {
                // TODO: Drop the settings parameter from the delegate?
                new AssignHtmlTemplateRenderer(settings => new RemoteHtmlTemplateBundleRenderer(urlGenerator)),
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