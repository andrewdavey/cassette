using System;
using System.Collections.Generic;
using System.IO;

namespace Cassette.Persistence
{
    public class CachedAssetSourceInfo : IAsset
    {
        public CachedAssetSourceInfo(string filename)
        {
            this.filename = filename;
        }

        readonly string filename;
        readonly List<AssetReference> references = new List<AssetReference>();

        public string SourceFilename
        {
            get { return filename; }
        }

        public void Accept(IAssetVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<AssetReference> References
        {
            get { return references; }
        }

        public void AddReferences(IEnumerable<AssetReference> references)
        {
            this.references.AddRange(references);
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

        public Stream OpenStream()
        {
            throw new NotImplementedException();
        }

        public byte[] Hash
        {
            get { throw new NotImplementedException(); }
        }


        public IFileSystem Directory
        {
            get { throw new NotImplementedException(); }
        }
    }
}
