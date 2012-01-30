using System;
using System.Collections.Generic;
using System.IO;
using Cassette.IO;

namespace Cassette
{
    class CachedBundleContent : IAsset
    {
        readonly IFile bundleContentFile;
        readonly IEnumerable<IAsset> originalAssets;

        public CachedBundleContent(IFile bundleContentFile, IEnumerable<IAsset> originalAssets)
        {
            this.bundleContentFile = bundleContentFile;
            this.originalAssets = originalAssets;
        }

        public byte[] Hash
        {
            get { throw new NotImplementedException(); }
        }

        public IFile SourceFile
        {
            get { return bundleContentFile; }
        }

        public IEnumerable<AssetReference> References
        {
            get { throw new NotImplementedException(); }
        }

        public void Accept(IBundleVisitor visitor)
        {
            foreach (var originalAsset in originalAssets)
            {
                originalAsset.Accept(visitor);
            }
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            throw new NotImplementedException();
        }

        public void AddReference(string assetRelativePath, int lineNumber)
        {
            throw new NotImplementedException();
        }

        public void AddRawFileReference(string relativeFilename)
        {
            throw new NotImplementedException();
        }

        public Stream OpenStream()
        {
            return bundleContentFile.OpenRead();
        }
    }
}