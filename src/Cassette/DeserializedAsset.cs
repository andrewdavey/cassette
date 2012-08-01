using System;
using System.Collections.Generic;
using System.IO;

namespace Cassette
{
    /// <summary>
    /// DeserializedAsset implements enough of <see cref="IAsset"/> to work with reference path resolution.
    /// It only supports Path, AssetCacheValidatorType, References and Accept.
    /// </summary>
    class DeserializedAsset : IAsset
    {
        readonly string path;
        readonly IEnumerable<AssetReference> references;
        readonly Type assetCacheValidatorType;

        public DeserializedAsset(string path, IEnumerable<AssetReference> references, Type assetCacheValidatorType)
        {
            this.path = path;
            this.references = references;
            this.assetCacheValidatorType = assetCacheValidatorType;
        }

        public string Path
        {
            get { return path; }
        }

        public void Accept(IBundleVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<AssetReference> References
        {
            get { return references; }
        }

        public Type AssetCacheValidatorType
        {
            get { return assetCacheValidatorType; }
        }

        public byte[] Hash
        {
            get { throw new NotSupportedException(); }
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
            throw new NotSupportedException();
        }
    }
}