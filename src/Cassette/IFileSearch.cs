using System.Collections.Generic;
using Cassette.IO;

namespace Cassette
{
    public interface IFileSearch
    {
        IEnumerable<IFile> FindFiles(IDirectory directory);
    }
}