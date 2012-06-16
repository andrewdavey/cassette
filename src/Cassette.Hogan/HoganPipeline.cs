using Cassette.BundleProcessing;
using Cassette.TinyIoC;

namespace Cassette.HtmlTemplates
{
    public class HoganPipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public HoganPipeline(TinyIoCContainer container, HoganSettings hoganSettings)
            : base(container)
        {
            var renderer = container.Resolve<RemoteHtmlTemplateBundleRenderer>();
            AddRange(new IBundleProcessor<HtmlTemplateBundle>[]
            {
                new AssignHtmlTemplateRenderer(renderer),
                new AssignContentType("text/javascript"),
                new ParseHtmlTemplateReferences(),
                container.Resolve<CompileHogan>(),
                new RegisterTemplatesWithHogan(hoganSettings.JavaScriptVariableName),
                new AssignHash(),
                new ConcatenateAssets()
            });
        }
    }
}