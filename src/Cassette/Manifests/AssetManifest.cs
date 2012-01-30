using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.IO;

namespace Cassette.Manifests
{
    class AssetManifest
    {
        public AssetManifest()
        {
            References = new List<AssetReferenceManifest>();
        }

        public string Path { get; set; }
        public IList<AssetReferenceManifest> References { get; private set; }
        
        public override bool Equals(object obj)
        {
            var other = obj as AssetManifest;
            return other != null && Path.Equals(other.Path);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public bool IsUpToDateWithFileSystem(IDirectory directory, DateTime asOfDateTime)
        {
            var file = directory.GetFile(Path);
            return FileIsUpToDateWithFileSystem(file, asOfDateTime)
                && AllRawFileReferencesAreUpToDateWithFileSystem(directory, asOfDateTime);
        }

        bool AllRawFileReferencesAreUpToDateWithFileSystem(IDirectory directory, DateTime asOfDateTime)
        {
            var files = RawFilenameReferences().Select(r => directory.GetFile(r.Path));
            return files.All(file => FileIsUpToDateWithFileSystem(file, asOfDateTime));
        }

        IEnumerable<AssetReferenceManifest> RawFilenameReferences()
        {
            return References.Where(r => r.Type == AssetReferenceType.RawFilename);
        }

        bool FileIsUpToDateWithFileSystem(IFile file, DateTime asOfDateTime)
        {
            return file.Exists 
                && file.LastWriteTimeUtc <= asOfDateTime;
        }
    }
}