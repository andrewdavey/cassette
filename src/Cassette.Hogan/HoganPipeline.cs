using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class HoganPipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public HoganPipeline(HoganSettings settings, IUrlGenerator urlGenerator)
        {
            AddRange(new IBundleProcessor<HtmlTemplateBundle>[]
            {
                new AssignHtmlTemplateRenderer(new RemoteHtmlTemplateBundleRenderer(urlGenerator)),
                new AssignContentType("text/javascript"),
                new ParseHtmlTemplateReferences(),
                new CompileHogan(),
                new RegisterTemplatesWithHogan(settings.JavaScriptVariableName),
                new AssignHash(),
                new ConcatenateAssets()
            });
        }
    }
}