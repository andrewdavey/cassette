using System;
using Cassette.BundleProcessing;
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
                container.Resolve<AddJavaScriptHtmlTemplateBuilders>(),
                new ConcatenateAssets { Separator = Environment.NewLine },
                new AddWrapJavaScriptHtmlTemplates(),
                container.Resolve<MinifyAssets>(),
                new AssignHash()
            });
        }
    }
}