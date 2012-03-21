using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class KnockoutJQueryTmplPipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public KnockoutJQueryTmplPipeline()
        {
            AddRange(new IBundleProcessor<HtmlTemplateBundle>[]
            {
                new AssignHtmlTemplateRenderer(settings => new RemoteHtmlTemplateBundleRenderer(settings.UrlGenerator)),
                new AssignContentType("text/javascript"),
                new ParseHtmlTemplateReferences(),
                new CompileKnockoutJQueryTmpl(),
                new RegisterTemplatesWithJQueryTmpl(),
                new AssignHash(),
                new ConcatenateAssets()
            });
        }
    }
}