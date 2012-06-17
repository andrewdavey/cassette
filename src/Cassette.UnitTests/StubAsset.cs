using System;
using System.Collections.Generic;
using System.IO;
using Cassette.Utilities;

namespace Cassette
{
    public class StubAsset : AssetBase
    {
        readonly byte[] hash;
        readonly string path;

        public StubAsset(string fullPath = "~/asset.js", string content = "", byte[] hash = null)
        {
            this.hash = hash ?? new byte[] {1};
            CreateStream = () => content.AsStream();
            path = fullPath;
            ReferenceList = new List<AssetReference>();
        }

        public Func<Stream> CreateStream { get; set; }
 
        public override byte[] Hash
        {
            get { return hash; }
        }

        public override string Path
        {
            get { return path; }
        }

        public List<AssetReference> ReferenceList { get; set; }

        public override IEnumerable<AssetReference> References
        {
            get { return ReferenceList; }
        }

        public override void Accept(IBundleVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override void AddReference(string assetRelativePath, int lineNumber)
        {
        }

        public override void AddRawFileReference(string relativeFilename)
        {
            ReferenceList.Add(new AssetReference(Path, relativeFilename, -1, AssetReferenceType.RawFilename));
        }

        public override Type AssetCacheValidatorType
        {
            get { return typeof(Caching.FileAssetCacheValidator); }
        }

        protected override Stream OpenStreamCore()
        {
            return CreateStream();
        }
    }
}