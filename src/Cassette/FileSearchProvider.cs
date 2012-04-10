using System;
using Cassette.Configuration;

namespace Cassette
{
    class FileSearchProvider : IFileSearchProvider
    {
        readonly Func<Type, IFileSearch> getFileSearchForBundleType;

        public FileSearchProvider(Func<Type, IFileSearch> getFileSearchForBundleType)
        {
            this.getFileSearchForBundleType = getFileSearchForBundleType;
        }

        public IFileSearch GetFileSearch(Type bundleType)
        {
            return getFileSearchForBundleType(bundleType);
        }
    }
}