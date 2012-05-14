using System.Collections.Generic;
using Cassette.IO;

namespace Cassette
{
    public interface IBundleFactory<out T>
        where T : Bundle
    {
        T CreateBundle(string path, IEnumerable<IFile> allFiles, BundleDescriptor bundleDescriptor);
    }
}