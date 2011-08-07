using System.Collections.Generic;
using System.IO;

namespace Cassette
{
    class StubFileSystem : IFileSystem
    {
        private Dictionary<string, Stream> fileStreams;

        public StubFileSystem(Dictionary<string, Stream> fileStreams)
        {
            this.fileStreams = fileStreams;
        }

        public Stream OpenRead(string filename)
        {
            return fileStreams[filename];
        }

        public Stream OpenWrite(string filename)
        {
            throw new System.NotImplementedException();
        }

        public bool FileExists(string filename)
        {
            return fileStreams.ContainsKey(filename);
        }

        public void DeleteAll()
        {
            fileStreams.Clear();
        }
    }
}
