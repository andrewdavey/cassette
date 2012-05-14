﻿using System;
using System.Collections.Generic;
using System.IO;
using Cassette.Caching;

namespace Cassette
{
    public interface IAsset
    {
        /// <summary>
        /// Gets a type of <see cref="IAssetCacheValidator"/> used to validate if a cache.
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
