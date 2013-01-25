using Cassette.Caching;
using System;
using System.Collections.Generic;

namespace Cassette
{
    public interface IAsset
    {
        /// <summary>
        /// Gets a type of <see cref="IAssetCacheValidator"/> used to validate if a cache is valid.
        /// </summary>
        Type AssetCacheValidatorType { get; }

        /// <summary>
        /// Gets the hash of the transformed asset content.
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

        /// <summary>
        /// Adds an asset transformer that will transform this asset.
        /// </summary>
        void AddAssetTransformer(IAssetTransformer transformer);

        /// <summary>
        /// Adds a reference to another asset. This is used to determine the sort order of assets.
        /// </summary>
        void AddReference(string assetRelativePath, int lineNumber);

        /// <summary>
        /// Adds a reference to non-asset file, such as an image referenced by a stylesheet.
        /// </summary>
        void AddRawFileReference(string relativeFilename);

        /// <summary>
        /// Gets the content of this asset, after all asset tranformers have been applied.
        /// </summary>
        string GetTransformedContent();
    }
}
