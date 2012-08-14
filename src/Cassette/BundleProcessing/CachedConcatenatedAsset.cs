using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.BundleProcessing
{
    class CachedConcatenatedAsset : ConcatenatedAsset
    {
        readonly IFile file;  

        public CachedConcatenatedAsset(IFile file, FileAsset fileAsset)
            : base(new IAsset[] {fileAsset})
        {
            this.file = file;
        }

        public override IO.IFile SourceFile
        {
            get { return file; }
        }
    }
}
