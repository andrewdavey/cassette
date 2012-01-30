using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;

namespace Cassette.Manifests
{
    class CachedBundleContent : IAsset
    {
        readonly byte[] content;
        readonly IEnumerable<IAsset> originalAssets;

        public CachedBundleContent(byte[] content, IEnumerable<IAsset> originalAssets)
        {
            this.content = content;
            this.originalAssets = originalAssets.ToArray();
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
            if (content == null)
            {
                throw new InvalidOperationException("Cannot open stream. Bundle was created from a manifest without any content.");
            }
            return new MemoryStream(content);
        }

        public byte[] Hash
        {
            get { throw new NotImplementedException(); }
        }

        public IFile SourceFile
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