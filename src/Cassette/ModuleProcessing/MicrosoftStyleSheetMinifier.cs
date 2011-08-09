using System;
using System.IO;
using Microsoft.Ajax.Utilities;
using Cassette.Utilities;

namespace Cassette.ModuleProcessing
{
    public class MicrosoftStyleSheetMinifier : IAssetTransformer
    {
        public MicrosoftStyleSheetMinifier()
            : this(new CssSettings())
        {
        }

        public MicrosoftStyleSheetMinifier(CssSettings cssSettings)
        {
            this.cssSettings = cssSettings;
        }

        readonly CssSettings cssSettings;

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                using (var reader = new StreamReader(openSourceStream()))
                {
                    var output = new Minifier().MinifyStyleSheet(reader.ReadToEnd(), cssSettings);
                    return output.AsStream();
                }
            };
        }
    }
}