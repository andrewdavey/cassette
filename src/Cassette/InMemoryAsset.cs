using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cassette
{
    class InMemoryAsset : AssetBase, IDisposable
    {
        readonly IEnumerable<string> sourceFilenames;
        readonly Stream stream;
        readonly IEnumerable<AssetReference> references;

        public InMemoryAsset(IEnumerable<string> sourceFilenames, Stream stream, IEnumerable<AssetReference> references)
        {
            this.sourceFilenames = sourceFilenames;
            this.stream = stream;
            this.references = references;
        }

        public override string SourceFilename
        {
            get { throw new NotImplementedException(); }
        }

        public override IEnumerable<AssetReference> References
        {
            get { return references; }
        }

        public override void AddReference(string path, int lineNumber)
        {
            throw new NotImplementedException();
        }

        protected override Stream OpenStreamCore()
        {
            // TODO: Clone the stream and return...
            return stream;
        }

        public override bool IsFrom(string path)
        {
            return sourceFilenames.Any(f => f.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        public void Dispose()
        {
            stream.Dispose();
        }
    }
}
