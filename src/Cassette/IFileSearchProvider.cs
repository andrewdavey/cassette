using System;
using Cassette.Configuration;

namespace Cassette
{
    public interface IFileSearchProvider
    {
        IFileSearch GetFileSearch(Type bundleType);
    }
}