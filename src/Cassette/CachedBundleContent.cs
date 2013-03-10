using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;

namespace Cassette
{
    class CachedBundleContent : IAsset
    {
        readonly IEnumerable<IAsset> originalAssets;
        readonly IFile file;

        public CachedBundleContent(IFile file, IEnumerable<IAsset> originalAssets)
        {
            this.file = file;
            this.originalAssets = originalAssets.ToArray();
        }

        public IEnumerable<IAsset> OriginalAssets
        {
            get { return originalAssets; }
        }

        public void Accept(IBundleVisitor visitor)
        {
            foreach (var originalAsset in originalAssets)
            {
                originalAsset.Accept(visitor);
            }
        }

        public Stream OpenStream()
        {
            if (file == null)
            {
                throw new InvalidOperationException("Cannot open stream. Bundle was created from a manifest without any content.");
            }
            return file.OpenRead();
        }

        public Type AssetCacheValidatorType
        {
            get { throw new NotImplementedException(); }
        }

        public byte[] Hash
        {
            get { throw new NotImplementedException(); }
        }

        public string Path
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<AssetReference> References
        {
            get { throw new NotImplementedException(); }
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
    }
}