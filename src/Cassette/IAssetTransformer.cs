using System;
using System.IO;

namespace Cassette
{
    /// <summary>
    /// Transforms asset content.
    /// </summary>
    public interface IAssetTransformer
    {
        /// <summary>
        /// Returns a function that will transform an asset's content stream.
        /// </summary>
        /// <param name="openSourceStream">A function that opens a stream to the asset's content.</param>
        /// <param name="asset">The asset being transformed.</param>
        /// <returns>A function that returns the transformed content stream.</returns>
        Func<Stream> Transform(Func<Stream> openSourceStream, IAsset asset);
    }
}