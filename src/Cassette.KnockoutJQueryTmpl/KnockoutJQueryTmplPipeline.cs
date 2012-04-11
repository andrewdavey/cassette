using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class KnockoutJQueryTmplPipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public KnockoutJQueryTmplPipeline(IUrlGenerator urlGenerator)
        {
            AddRange(new IBundleProcessor<HtmlTemplateBundle>[]
            {
                new AssignHtmlTemplateRenderer(new RemoteHtmlTemplateBundleRenderer(urlGenerator)),
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