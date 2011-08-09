using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;

namespace Cassette.Web
{
    public class CassetteApplication : Cassette.CassetteApplication
    {
        public CassetteApplication(string rootDirectory, string virtualDirectory, IFileSystem cacheFileSystem)
            : base(new FileSystem(rootDirectory), cacheFileSystem)
        {
        }

    }
}
