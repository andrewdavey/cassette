using System;
using System.Collections.Generic;
using System.IO;
using Cassette.Utilities;

namespace Cassette
{
    public class StubAsset : IAsset
    {
        public StubAsset(string fullPath = "~/asset.js", string content = "")
        {
            Hash = new byte[] {1};
            CreateStream = () => content.AsStream();
            Path = fullPath;
            References = new List<AssetReference>();
        }

        public Func<Stream> CreateStream { get; set; }
 
        public byte[] Hash { get; set; }

        public string Path { get; set; }

        public List<AssetReference> References { get; set; }

        IEnumerable<AssetReference> IAsset.References
        {
            get { return References; }
        }

        public void Accept(IBundleVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
        }

        public void AddReference(string assetRelativePath, int lineNumber)
        {
        }

        public void AddRawFileReference(string relativeFilename)
        {
            References.Add(new AssetReference(Path, relativeFilename, -1, AssetReferenceType.RawFilename));
        }

        public Stream OpenStream()
        {
            return CreateStream();
        }

        public Type AssetCacheValidatorType
        {
            get { throw new NotImplementedException(); }
        }
    }
}