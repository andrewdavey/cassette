using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.IO;

namespace Cassette
{
    class StubFileSystem : IDirectory
    {
        private readonly Dictionary<string, Stream> fileStreams;

        public StubFileSystem(Dictionary<string, Stream> fileStreams)
        {
            this.fileStreams = fileStreams;
        }

        public void DeleteContents()
        {
            fileStreams.Clear();
        }

        public IDirectory GetDirectory(string path)
        {
            return new StubFileSystem(
                fileStreams
                    .Where(f => f.Key.StartsWith(path))
                    .ToDictionary(
                        kvp => kvp.Key.Substring(path.Length == 0 ? 0 : path.Length + 1),
                        kvp => kvp.Value
                    )
                );
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDirectory> GetDirectories()
        {
            throw new NotImplementedException();
        }

        public string FullPath
        {
            get { return "~/"; }
        }

        public IFile GetFile(string filename)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IFile> GetFiles(string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public FileAttributes Attributes
        {
            get { return FileAttributes.Directory; }
        }
    }
}

