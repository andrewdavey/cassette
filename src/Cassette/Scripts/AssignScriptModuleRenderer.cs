using Cassette.ModuleProcessing;

namespace Cassette.Scripts
{
    public class AssignScriptModuleRenderer : IModuleProcessor<ScriptModule>
    {
        public void Process(ScriptModule module, ICassetteApplication application)
        {
            if (application.IsOutputOptimized)
            {
                module.Renderer = new ScriptModuleHtmlRenderer(application.UrlGenerator);
            }
            else
            {
                module.Renderer = new DebugScriptModuleHtmlRenderer(application.UrlGenerator);
            }
        }
    }
}