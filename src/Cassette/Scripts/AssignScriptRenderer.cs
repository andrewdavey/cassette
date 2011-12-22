using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    public class AssignScriptRenderer : IBundleProcessor<ScriptBundle>
    {
        public void Process(ScriptBundle bundle, CassetteSettings settings)
        {
            if (settings.IsDebuggingEnabled)
            {
                bundle.Renderer = new DebugScriptBundleHtmlRenderer(settings.UrlGenerator);
            }
            else
            {
                bundle.Renderer = new ScriptBundleHtmlRenderer(settings.UrlGenerator);
            }
        }
    }
}