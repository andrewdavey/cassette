using System;
using System.IO;

namespace Cassette
{
    public interface IAssetTransformer
    {
        Func<Stream> Transform(Func<Stream> content, IAsset asset);
    }
}
