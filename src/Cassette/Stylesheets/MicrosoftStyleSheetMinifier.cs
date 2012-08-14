using System;
using System.IO;
using Cassette.Utilities;
using Microsoft.Ajax.Utilities;

namespace Cassette.Stylesheets
{
    [ProtoBuf.ProtoContract]
    public class MicrosoftStylesheetMinifier : IAssetTransformer
    {
        public MicrosoftStylesheetMinifier()
            : this(new CssSettings())
        {
        }

        public MicrosoftStylesheetMinifier(CssSettings cssSettings)
        {
            this.cssSettings = cssSettings;
        }

        [ProtoBuf.ProtoMember(1)]
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
