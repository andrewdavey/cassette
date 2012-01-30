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
            RawFileReferences = new List<string>();
        }

        public string Path { get; set; }
        public IList<string> RawFileReferences { get; private set; }

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
            var files = RawFileReferences.Select(directory.GetFile);
            return files.All(file => FileIsUpToDateWithFileSystem(file, asOfDateTime));
        }

        bool FileIsUpToDateWithFileSystem(IFile file, DateTime asOfDateTime)
        {
            return file.Exists 
                && file.LastWriteTimeUtc <= asOfDateTime;
        }
    }
}