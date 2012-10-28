using Microsoft.Ajax.Utilities;

namespace Cassette.Stylesheets
{
    public class MicrosoftStylesheetMinifier : IStylesheetMinifier
    {
        public MicrosoftStylesheetMinifier()
            : this(new CssSettings())
        {
        }

        public MicrosoftStylesheetMinifier(CssSettings cssSettings)
        {
            this.cssSettings = cssSettings;
        }

        readonly CssSettings cssSettings;

        public string Transform(string assetContent, IAsset asset)
        {
            return new Minifier().MinifyStyleSheet(assetContent, cssSettings);
        }
    }
}