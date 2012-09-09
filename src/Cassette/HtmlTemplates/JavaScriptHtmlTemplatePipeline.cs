using System;
using Cassette.BundleProcessing;
using Cassette.Scripts;
using Cassette.TinyIoC;

namespace Cassette.HtmlTemplates
{
    public class JavaScriptHtmlTemplatePipeline : BundlePipeline<HtmlTemplateBundle>
    {
        public JavaScriptHtmlTemplatePipeline(TinyIoCContainer container)
            : base(container)
        {
            AddRange(new IBundleProcessor<HtmlTemplateBundle>[]
            {
                container.Resolve<AddHtmlTemplateToJavaScriptTransformers>(),
                new ConcatenateAssets { Separator = Environment.NewLine },
                new AddWrapJavaScriptHtmlTemplates(),
                new MinifyAssets(container.Resolve<IJavaScriptMinifier>()),
                new AssignHash(),
                new AssignContentType("text/javascript"),
                new AssignHtmlTemplateRenderer(container.Resolve<RemoteHtmlTemplateBundleRenderer>()), 
            });
        }
    }
}