﻿using System;
using System.IO;

namespace Cassette
{
    public class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(path);
        }

        readonly string path;

        public static implicit operator string(TempDirectory directory)
        {
            return directory.path;
        }

        public void Dispose()
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }
}
