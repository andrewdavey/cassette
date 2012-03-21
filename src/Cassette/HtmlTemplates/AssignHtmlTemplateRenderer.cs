using System;
using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class AssignHtmlTemplateRenderer : IBundleProcessor<HtmlTemplateBundle>
    {
        readonly Func<CassetteSettings, IBundleHtmlRenderer<HtmlTemplateBundle>> createRenderer;

        public AssignHtmlTemplateRenderer(Func<CassetteSettings, IBundleHtmlRenderer<HtmlTemplateBundle>> createRenderer)
        {
            this.createRenderer = createRenderer;
        }

        public void Process(HtmlTemplateBundle bundle, CassetteSettings settings)
        {
            bundle.Renderer = createRenderer(settings);
        }
    }
}