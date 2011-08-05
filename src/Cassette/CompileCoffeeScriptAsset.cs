using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cassette
{
    public class CompileCoffeeScriptAsset : IAssetTransformer
    {
        public Func<Stream> Transform(Func<Stream> content, IAsset asset)
        {
            throw new NotImplementedException();
        }
    }
}
