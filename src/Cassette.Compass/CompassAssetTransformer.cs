using System;
using System.IO;

namespace Cassette.Compass
{
    public class CompassAssetTransformer : IAssetTransformer
    {
        readonly string compassOutputFilePath;

        public CompassAssetTransformer(string compassOutputFilePath)
        {
            this.compassOutputFilePath = compassOutputFilePath;
        }

        public Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset)
        {
            // sorry to say, since Compass already batch-compiled all of the assets we don't really care about that nice open stream that Cassette passed us
            // so we'll send back a stream of the sass'd css file we passed to the constructor
            return delegate { return File.OpenRead(compassOutputFilePath); };
        }
    }
}
