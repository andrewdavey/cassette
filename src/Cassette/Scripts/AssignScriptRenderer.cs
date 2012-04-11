using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Scripts
{
    public class AssignScriptRenderer : IBundleProcessor<ScriptBundle>
    {
        readonly IUrlGenerator urlGenerator;

        public AssignScriptRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        public void Process(ScriptBundle bundle, CassetteSettings settings)
        {
            if (settings.IsDebuggingEnabled)
            {
                bundle.Renderer = new DebugScriptBundleHtmlRenderer(urlGenerator);
            }
            else
            {
                bundle.Renderer = new ScriptBundleHtmlRenderer(urlGenerator);
            }
        }
    }
}