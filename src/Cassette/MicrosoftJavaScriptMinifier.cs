using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cassette
{
    public class MicrosoftJavaScriptMinifier : IAssetTransformer
    {
        public Func<System.IO.Stream> Transform(Func<System.IO.Stream> openSourceStream, IAsset asset)
        {
            throw new NotImplementedException();
        }
    }
}
