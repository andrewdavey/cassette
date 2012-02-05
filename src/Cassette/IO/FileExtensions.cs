﻿using System.IO;

namespace Cassette.IO
{
    static class FileExtensions
    {
        public static Stream OpenRead(this IFile file)
        {
            return file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public static byte[] ReadFully(this IFile file)
        {
            using (var ms = new MemoryStream())
            {
                file.OpenRead().CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
