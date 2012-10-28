using Microsoft.Ajax.Utilities;

namespace Cassette.Scripts
{
    public class MicrosoftJavaScriptMinifier : IJavaScriptMinifier
    {
        public MicrosoftJavaScriptMinifier()
            : this(new CodeSettings())
        {
        }

        public MicrosoftJavaScriptMinifier(CodeSettings codeSettings)
        {
            this.codeSettings = codeSettings;
        }

        readonly CodeSettings codeSettings;

        public string Transform(string assetContent, IAsset asset)
        {
            return new Minifier().MinifyJavaScript(assetContent, codeSettings);
        }
    }
}