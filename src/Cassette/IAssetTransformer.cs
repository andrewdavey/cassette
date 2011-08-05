using System;
using System.IO;

namespace Cassette
{
    public interface IAssetTransformer
    {
        Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset);
    }
}
