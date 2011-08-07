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

        public Stream OpenFile(string filename, FileMode mode, FileAccess access)
        {
            return fileStreams[filename];
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
