using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cassette
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
            get { throw new NotImplementedException(); }
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

        public bool IsFrom(string filename)
        {
            return this.filename.Equals(filename, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<System.Xml.Linq.XElement> CreateManifest()
        {
            throw new NotImplementedException();
        }
    }
}
