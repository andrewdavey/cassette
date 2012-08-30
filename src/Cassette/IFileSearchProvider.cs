using System;

namespace Cassette
{
    public interface IFileSearchProvider
    {
        IFileSearch GetFileSearch(Type bundleType);
    }
}