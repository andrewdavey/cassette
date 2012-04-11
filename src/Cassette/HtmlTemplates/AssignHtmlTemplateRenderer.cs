using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class AssignHtmlTemplateRenderer : IBundleProcessor<HtmlTemplateBundle>
    {
        readonly IBundleHtmlRenderer<HtmlTemplateBundle> renderer;

        public AssignHtmlTemplateRenderer(IBundleHtmlRenderer<HtmlTemplateBundle> renderer)
        {
            this.renderer = renderer;
        }

        public void Process(HtmlTemplateBundle bundle, CassetteSettings settings)
        {
            bundle.Renderer = renderer;
        }
    }
}