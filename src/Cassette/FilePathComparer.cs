using System;
using System.Collections.Generic;
using Cassette.IO;

namespace Cassette
{
    class FilePathComparer : IEqualityComparer<IFile>
    {
        public bool Equals(IFile x, IFile y)
        {
            return x.FullPath.Equals(y.FullPath, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(IFile obj)
        {
            return obj.FullPath.GetHashCode();
        }
    }
}