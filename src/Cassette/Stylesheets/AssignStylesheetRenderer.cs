using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class AssignStylesheetRenderer : IBundleProcessor<StylesheetBundle>
    {
        readonly IUrlGenerator urlGenerator;

        public AssignStylesheetRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        public void Process(StylesheetBundle bundle, CassetteSettings settings)
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