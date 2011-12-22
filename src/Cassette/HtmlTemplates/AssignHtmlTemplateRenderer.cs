using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class AssignHtmlTemplateRenderer : IBundleProcessor<HtmlTemplateBundle>
    {
        public AssignHtmlTemplateRenderer(IBundleHtmlRenderer<HtmlTemplateBundle> renderer)
        {
            this.renderer = renderer;
        }

        readonly IBundleHtmlRenderer<HtmlTemplateBundle> renderer;

        public void Process(HtmlTemplateBundle bundle, CassetteSettings settings)
        {
            bundle.Renderer = renderer;
        }
    }
}