using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Cassette
{
    public class ConcatenatedAsset : AssetBase, IDisposable
    {
        readonly IEnumerable<IAsset> children;
        readonly Stream stream;
        readonly IEnumerable<AssetReference> references;

        public ConcatenatedAsset(IEnumerable<IAsset> children, Stream stream)
        {
            this.children = children;
            this.stream = stream;
        }

        public override string SourceFilename
        {
            get { return string.Join(";", children.Select(c => c.SourceFilename)); }
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

        public override bool IsFrom(string path)
        {
            return children.Any(c => c.IsFrom(path));
        }

        public override IEnumerable<XElement> CreateManifest()
        {
            return children.SelectMany(c => c.CreateManifest());
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}
