using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette.Manifests
{
    class AssetFromManifest : IAsset
    {
        readonly AssetManifest assetManifest;

        public AssetFromManifest(AssetManifest assetManifest)
        {
            this.assetManifest = assetManifest;
        }

        public byte[] Hash
        {
            get { throw new NotImplementedException(); }
        }

        public string Path
        {
            get { return assetManifest.Path; }
        }

        public IEnumerable<AssetReference> References
        {
            get
            {
                return assetManifest.References.Select(
                    r => new AssetReference(r.Path, this, r.SourceLineNumber, r.Type)
                );
            }
        }

        public void Accept(IBundleVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            throw new NotImplementedException();
        }

        public void AddReference(string assetRelativePath, int lineNumber)
        {
            throw new NotImplementedException();
        }

        public void AddRawFileReference(string relativeFilename)
        {
            throw new NotImplementedException();
        }

        public Stream OpenStream()
        {
            throw new NotImplementedException();
        }
    }
}