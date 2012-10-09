using System.Collections.Generic;
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
            
            Add(new AssignHtmlTemplateRenderer(renderer));
            Add(new AssignContentType("text/javascript"));
            Add<ParseHtmlTemplateReferences>();
            Add<CompileHogan>();
            Add<RegisterTemplatesWithHogan.Factory>(create => create(hoganSettings.JavaScriptVariableName));
            Add<AssignHash>();
            Add<ConcatenateAssets>();
        }
    }
}