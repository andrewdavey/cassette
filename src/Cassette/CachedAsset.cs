using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette
{
    public class CachedAsset : IAsset
    {
        public CachedAsset(IEnumerable<IAsset> assetInfos, Func<Stream> openStream)
        {
            this.assetInfos = assetInfos.ToArray();
            this.openStream = openStream;
        }

        readonly IEnumerable<IAsset> assetInfos;
        readonly Func<Stream> openStream;
        readonly List<AssetReference> references = new List<AssetReference>();

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

        public System.IO.Stream OpenStream()
        {
            return openStream();
        }

        public bool IsFrom(string filename)
        {
            return assetInfos.Any(a => a.IsFrom(filename));
        }

        public IEnumerable<System.Xml.Linq.XElement> CreateManifest()
        {
            throw new NotImplementedException();
        }
    }
}
