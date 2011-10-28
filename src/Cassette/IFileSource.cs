using System.Collections.Generic;
using Cassette.IO;

namespace Cassette
{
    public interface IFileSource
    {
        IEnumerable<IFile> GetFiles(IDirectory directory);
    }
}