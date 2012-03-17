using System.Collections.Generic;
using System.IO;

namespace Cassette
{
    public interface IAsset
    {
        /// <summary>
        /// Gets the hash of the original asset contents, before any transformations are applied.
        /// </summary>
        byte[] Hash { get; }

        /// <summary>
        /// Gets the application relative path of the asset.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the references made by this asset.
        /// </summary>
        IEnumerable<AssetReference> References { get; }
        
        void Accept(IBundleVisitor visitor);

        void AddAssetTransformer(IAssetTransformer transformer);

        void AddReference(string assetRelativePath, int lineNumber);

        void AddRawFileReference(string relativeFilename);

        /// <summary>
        /// Opens a new stream to read the transformed contents of the asset.
        /// </summary>
        /// <returns>A readable <see cref="Stream"/>.</returns>
        Stream OpenStream();
    }
}
