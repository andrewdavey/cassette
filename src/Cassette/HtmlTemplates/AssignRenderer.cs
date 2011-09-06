using Cassette.ModuleProcessing;

namespace Cassette.HtmlTemplates
{
    public class AssignRenderer : IModuleProcessor<HtmlTemplateModule>
    {
        public AssignRenderer(IModuleHtmlRenderer<HtmlTemplateModule> renderer)
        {
            this.renderer = renderer;
        }

        readonly IModuleHtmlRenderer<HtmlTemplateModule> renderer;

        public void Process(HtmlTemplateModule module, ICassetteApplication application)
        {
            module.Renderer = renderer;
        }
    }
}