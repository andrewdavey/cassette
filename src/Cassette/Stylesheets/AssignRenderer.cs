using Cassette.ModuleProcessing;

namespace Cassette.Stylesheets
{
    public class AssignRenderer : IModuleProcessor<StylesheetModule>
    {
        public void Process(StylesheetModule module, ICassetteApplication application)
        {
            if (application.IsOutputOptimized)
            {
                module.Renderer = new StylesheetHtmlRenderer(application.UrlGenerator);
            }
            else
            {
                module.Renderer = new DebugStylesheetHtmlRenderer(application.UrlGenerator);
            }
        }
    }
}