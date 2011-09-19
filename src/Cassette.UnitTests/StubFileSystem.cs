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

        public IDirectory GetDirectory(string path, bool createIfNotExists)
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

        public IEnumerable<string> GetDirectoryPaths(string relativePath)
        {
            throw new NotImplementedException();
        }

        public IFile GetFile(string filename)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFilePaths(string directory, SearchOption searchOption, string searchPattern)
        {
            if (searchPattern == "*")
            {
                return fileStreams.Keys;
            }
            else
            {
                return fileStreams.Keys.Where(key => key.EndsWith(searchPattern.Substring(1)));
            }
        }

        public FileAttributes GetAttributes(string path)
        {
            throw new NotImplementedException();
        }
    }
}
