using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Cassette
{
    class InMemoryAsset : IAsset, IDisposable
    {
        readonly Stream stream;

        public InMemoryAsset(Stream stream)
        {
            this.stream = stream;
        }

        public string SourceFilename
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<AssetReference> References
        {
            get { throw new NotImplementedException(); }
        }

        public void AddReference(string path)
        {
            throw new NotImplementedException();
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream OpenStream()
        {
            // TODO: Clone the stream and return...
            return stream;
        }

        public void Dispose()
        {
            stream.Dispose();
        }


    }
}
