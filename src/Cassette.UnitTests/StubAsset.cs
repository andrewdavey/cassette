using System;
using System.Collections.Generic;
using System.IO;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette
{
    class StubAsset : IAsset
    {
        public StubAsset(string fullPath = "~/asset.js", string content = "")
        {
            Hash = new byte[] {1};
            CreateStream = () => content.AsStream();
            SourceFile = new StubFile { FullPath = fullPath };
            References = new List<AssetReference>();
        }

        public Func<Stream> CreateStream { get; set; }
 
        public byte[] Hash { get; set; }

        public IFile SourceFile { get; set; }

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
        }

        public Stream OpenStream()
        {
            return CreateStream();
        }

        class StubFile : IFile
        {
            public IDirectory Directory
            {
                get { throw new NotImplementedException(); }
            }

            public bool Exists
            {
                get { throw new NotImplementedException(); }
            }

            public DateTime LastWriteTimeUtc
            {
                get { throw new NotImplementedException(); }
            }

            public string FullPath { get; set; }

            public Stream Open(FileMode mode, FileAccess access, FileShare fileShare)
            {
                throw new NotImplementedException();
            }

            public void Delete()
            {
                throw new NotImplementedException();
            }
        }
    }
}
