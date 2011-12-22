using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;

namespace Cassette.Persistence
{
    class CachedAsset : IAsset
    {
        public CachedAsset(IFile file, byte[] hash, IEnumerable<IAsset> children)
        {
            this.file = file;
            this.hash = hash;
            this.children = children.ToArray();
        }

        readonly byte[] hash;
        readonly IFile file;
        readonly IEnumerable<IAsset> children;

        public IFile SourceFile
        {
            get { return file; }
        }

        public byte[] Hash
        {
            get { return hash; }
        }

        public IEnumerable<AssetReference> References
        {
            get { return children.SelectMany(asset => asset.References); }
        }

        public Stream OpenStream()
        {
            return file.OpenRead();
        }

        public void Accept(IBundleVisitor visitor)
        {
            foreach (var child in children)
            {
                child.Accept(visitor);
            }
        }

        public void AddReference(string path, int lineNumber)
        {
            throw new NotImplementedException();
        }

        public void AddRawFileReference(string relativeFilename)
        {
            throw new NotImplementedException();
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            throw new NotImplementedException();
        }
    }
}

