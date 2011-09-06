using System;
using System.IO;

namespace Cassette.Persistence
{
    class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);
        }

        readonly string path;

        public static implicit operator string(TempDirectory directory)
        {
            return directory.path;
        }

        public void Dispose()
        {
            Directory.Delete(path, true);
        }
    }
}