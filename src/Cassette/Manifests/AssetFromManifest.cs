using System;
using System.Collections.Generic;
using System.IO;
using Cassette.IO;
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

        public IFile SourceFile
        {
            get { return new StubFile(assetManifest.Path); }
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

        class StubFile : IFile
        {
            readonly string fullPath;

            public StubFile(string fullPath)
            {
                this.fullPath = fullPath;
            }

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

            public string FullPath
            {
                get { return fullPath; }
            }

            public string FullSystemPath
            {
                get { throw new NotImplementedException(); }
            }

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