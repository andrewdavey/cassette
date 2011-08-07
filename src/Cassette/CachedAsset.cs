using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette
{
    public class CachedAsset : IAsset
    {
        public CachedAsset(IEnumerable<IAsset> children, Func<Stream> openStream)
        {
            this.children = children.ToArray();
            this.openStream = openStream;
        }

        readonly IEnumerable<IAsset> children;
        readonly Func<Stream> openStream;
        readonly List<AssetReference> references = new List<AssetReference>();

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

        public IEnumerable<AssetReference> References
        {
            get { return references; }
        }

        public void AddReference(string path, int lineNumber)
        {
            references.Add(new AssetReference(path, this, lineNumber, AssetReferenceType.DifferentModule));
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            throw new NotImplementedException();
        }

        public Stream OpenStream()
        {
            return openStream();
        }
    }
}
