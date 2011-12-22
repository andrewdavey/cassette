using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.Stylesheets
{
    public class AssignStylesheetRenderer : IBundleProcessor<StylesheetBundle>
    {
        public void Process(StylesheetBundle bundle, CassetteSettings settings)
        {
            if (settings.IsDebuggingEnabled)
            {
                bundle.Renderer = new DebugStylesheetHtmlRenderer(settings.UrlGenerator);
            }
            else
            {
                bundle.Renderer = new StylesheetHtmlRenderer(settings.UrlGenerator);
            }
        }
    }
}