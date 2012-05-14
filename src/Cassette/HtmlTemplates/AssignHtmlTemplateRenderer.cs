using Cassette.BundleProcessing;

namespace Cassette.HtmlTemplates
{
    public class AssignHtmlTemplateRenderer : IBundleProcessor<HtmlTemplateBundle>
    {
        public delegate AssignHtmlTemplateRenderer Factory(IBundleHtmlRenderer<HtmlTemplateBundle> renderer);

        readonly IBundleHtmlRenderer<HtmlTemplateBundle> renderer;

        public AssignHtmlTemplateRenderer(IBundleHtmlRenderer<HtmlTemplateBundle> renderer)
        {
            this.renderer = renderer;
        }

        public void Process(HtmlTemplateBundle bundle)
        {
            bundle.Renderer = renderer;
        }
    }
}