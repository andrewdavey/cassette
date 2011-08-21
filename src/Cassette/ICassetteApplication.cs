using System;
using Cassette.UI;

namespace Cassette
{
    public interface ICassetteApplication : IDisposable
    {
        IFileSystem RootDirectory { get; }
        bool IsOutputOptimized { get; }
        IUrlGenerator UrlGenerator { get; }

        IPageAssetManager GetPageAssetManager<T>() where T : Module;
    }
}