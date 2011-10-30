using System.Collections.Generic;
using Cassette.IO;

namespace Cassette.Configuration
{
    public interface IFileSearch
    {
        IEnumerable<IFile> FindFiles(IDirectory directory);
    }
}