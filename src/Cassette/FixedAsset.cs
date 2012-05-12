using System;
using System.Collections.Generic;
using System.IO;

namespace Cassette
{
    /// <summary>
    /// An asset based on another asset. The content is precomputed, so OpenStream() returns a fixed value without requiring the transformations to be applied each time.
    /// </summary>
    class FixedAsset : IAsset
    {
        readonly IAsset asset;
        readonly byte[] content;

        public FixedAsset(IAsset asset)
        {
            this.asset = asset;
            using (var stream = asset.OpenStream())
            using (var copy = new MemoryStream())
            {
                stream.CopyTo(copy);
                content = copy.ToArray();
            }
        }

        public Type AssetCacheValidatorType
        {
            get { return asset.AssetCacheValidatorType; }
        }

        public byte[] Hash
        {
            get { return asset.Hash; }
        }

        public string Path
        {
            get { return asset.Path; }
        }

        public IEnumerable<AssetReference> References
        {
            get { return asset.References; }
        }

        public void Accept(IBundleVisitor visitor)
        {
            asset.Accept(visitor);
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            throw new NotSupportedException();
        }

        public void AddReference(string assetRelativePath, int lineNumber)
        {
            throw new NotSupportedException();
        }

        public void AddRawFileReference(string relativeFilename)
        {
            throw new NotSupportedException();
        }

        public Stream OpenStream()
        {
            return new MemoryStream(content);
        }
    }
}