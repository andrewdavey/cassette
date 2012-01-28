using System;
using System.Collections.Generic;
using System.IO;
using Cassette.IO;

namespace Cassette.Scripts
{
    class ScriptBundleManifest : BundleManifest
    {
        public Bundle CreateBundle()
        {
            var bundle = new ScriptBundle(Path)
            {
                Hash = Hash, ContentType = ContentType, PageLocation = PageLocation, IsFromCache = true
            };
            AddAssetsToBundle(bundle);
            return bundle;
        }

        void AddAssetsToBundle(Bundle bundle)
        {
            foreach (var assetManifest in Assets)
            {
                bundle.Assets.Add(new AssetFromManifest(assetManifest.Path));
            }
        }
    }

    internal class AssetFromManifest : IAsset
    {
        readonly string path;

        public AssetFromManifest(string path)
        {
            this.path = path;
        }

        public byte[] Hash
        {
            get { throw new System.NotImplementedException(); }
        }

        public IFile SourceFile
        {
            get { return new StubFile(path); }
        }

        public IEnumerable<AssetReference> References
        {
            get { throw new System.NotImplementedException(); }
        }

        public void Accept(IBundleVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void AddAssetTransformer(IAssetTransformer transformer)
        {
            throw new System.NotImplementedException();
        }

        public void AddReference(string assetRelativePath, int lineNumber)
        {
            throw new System.NotImplementedException();
        }

        public void AddRawFileReference(string relativeFilename)
        {
            throw new System.NotImplementedException();
        }

        public Stream OpenStream()
        {
            throw new System.NotImplementedException();
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