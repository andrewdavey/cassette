using Cassette.BundleProcessing;

namespace Cassette.Scripts
{
    public class AssignScriptRenderer : IBundleProcessor<ScriptBundle>
    {
        readonly IUrlGenerator urlGenerator;
        readonly CassetteSettings settings;

        public AssignScriptRenderer(IUrlGenerator urlGenerator, CassetteSettings settings)
        {
            this.urlGenerator = urlGenerator;
            this.settings = settings;
        }

        public void Process(ScriptBundle bundle)
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