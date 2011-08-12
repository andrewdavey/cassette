using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.Utilities;

namespace Cassette
{
    public class ConcatenatedAsset : AssetBase, IDisposable
    {
        readonly byte[] hash;
        readonly IEnumerable<IAsset> children;
        readonly MemoryStream stream;

        public ConcatenatedAsset(IEnumerable<IAsset> children, MemoryStream stream)
        {
            this.hash = stream.ComputeSHA1Hash();
            this.children = children;
            this.stream = stream;
        }

        public override void Accept(IAssetVisitor visitor)
        {
            foreach (var child in children)
            {
                visitor.Visit(child);
            }
        }

        public override byte[] Hash
        {
            get { return hash; }
        }

        public override string SourceFilename
        {
            get { return string.Join(";", children.Select(c => c.SourceFilename)); }
        }

        public override IFileSystem Directory
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<AssetReference> References
        {
            get { return children.SelectMany(c => c.References); }
        }

        public override void AddReference(string path, int lineNumber)
        {
            throw new NotImplementedException();
        }

        protected override Stream OpenStreamCore()
        {
            var newStream = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo(newStream);
            newStream.Position = 0;
            return newStream;
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}
