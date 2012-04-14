using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class AssignStylesheetRenderer : IBundleProcessor<StylesheetBundle>
    {
        readonly IUrlGenerator urlGenerator;
        readonly CassetteSettings settings;

        public AssignStylesheetRenderer(IUrlGenerator urlGenerator, CassetteSettings settings)
        {
            this.urlGenerator = urlGenerator;
            this.settings = settings;
        }

        public void Process(StylesheetBundle bundle)
        {
            if (settings.IsDebuggingEnabled)
            {
                bundle.Renderer = new DebugStylesheetHtmlRenderer(urlGenerator);
            }
            else
            {
                bundle.Renderer = new StylesheetHtmlRenderer(urlGenerator);
            }
        }
    }
}