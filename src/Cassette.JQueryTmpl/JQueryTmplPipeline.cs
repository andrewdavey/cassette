using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class JQueryTmplPipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public JQueryTmplPipeline()
        {
            AddRange(new IBundleProcessor<HtmlTemplateBundle>[]
            {
                new AssignHtmlTemplateRenderer(settings => new RemoteHtmlTemplateBundleRenderer(settings.UrlGenerator)),
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