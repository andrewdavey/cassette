using Cassette.BundleProcessing;
using TinyIoC;

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
                container.Resolve<RegisterTemplatesWithHogan.Factory>()(hoganSettings.JavaScriptVariableName),
                new AssignHash(),
                new ConcatenateAssets()
            });
        }
    }
}