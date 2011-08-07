using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cassette.Persistence
{
    public class AssetInfo : IAsset
    {
        public AssetInfo(string filename)
        {
            this.filename = filename;
        }

        readonly string filename;

        public string SourceFilename
        {
            get { return filename; }
        }

        public IEnumerable<AssetReference> References
        {
            get { throw new NotImplementedException(); }
        }

        public void AddReference(string path, int lineNumber)
        {
            throw new NotImplementedException();
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            throw new NotImplementedException();
        }

        public System.IO.Stream OpenStream()
        {
            throw new NotImplementedException();
        }

        public void Accept(IAssetVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
