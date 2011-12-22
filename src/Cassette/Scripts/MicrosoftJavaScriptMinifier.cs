using System;
using System.IO;
using Cassette.Utilities;
using Microsoft.Ajax.Utilities;

namespace Cassette.Scripts
{
    public class MicrosoftJavaScriptMinifier : IAssetTransformer
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

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            return delegate
            {
                using (var reader = new StreamReader(openSourceStream()))
                {
                    var output = new Minifier().MinifyJavaScript(reader.ReadToEnd(), codeSettings);
                    return output.AsStream();
                }
            };
        }
    }
}

