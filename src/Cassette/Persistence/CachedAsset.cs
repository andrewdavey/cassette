using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette.Persistence
{
    public class CachedAsset : IAsset
    {
        public CachedAsset(string filename, byte[] hash, IEnumerable<IAsset> children, IFileSystem moduleDirectory)
        {
            this.filename = filename;
            this.hash = hash;
            this.moduleDirectory = moduleDirectory;
            this.children = children.ToArray();
        }

        readonly string filename;
        readonly byte[] hash;
        readonly IFileSystem moduleDirectory;
        readonly IEnumerable<IAsset> children;

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
            return moduleDirectory.OpenFile(filename, FileMode.Open, FileAccess.Read);
        }

        public void Accept(IAssetVisitor visitor)
        {
            foreach (var child in children)
            {
                visitor.Visit(child);
            }
        }


        public string SourceFilename
        {
            get { throw new NotImplementedException(); }
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

        public IFileSystem Directory
        {
            get { throw new NotImplementedException(); }
        }
    }
}
