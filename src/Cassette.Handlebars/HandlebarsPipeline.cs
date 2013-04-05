using System.Collections.Generic;
using Cassette.BundleProcessing;
using Cassette.TinyIoC;

namespace Cassette.HtmlTemplates
{
    public class HandlebarsPipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public HandlebarsPipeline(TinyIoCContainer container, HandlebarsSettings handlebarsSettings)
            : base(container)
        {
            var renderer = container.Resolve<RemoteHtmlTemplateBundleRenderer>();
            
            Add(new AssignHtmlTemplateRenderer(renderer));
            Add(new AssignContentType("text/javascript"));
            Add<ParseHtmlTemplateReferences>();
            Add<CompileHandlebars>();
            Add<RegisterTemplatesWithHandlebars.Factory>(create => create(handlebarsSettings.JavaScriptVariableName));
            Add<AssignHash>();
            Add<ConcatenateAssets>();
        }
    }
}