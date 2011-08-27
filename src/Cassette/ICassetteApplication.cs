using System;
using Cassette.UI;

namespace Cassette
{
    public interface ICassetteApplication : IDisposable
    {
        IFileSystem RootDirectory { get; }
        bool IsOutputOptimized { get; set; }
        IUrlGenerator UrlGenerator { get; set; }

        IPageAssetManager GetPageAssetManager<T>() where T : Module;
    }
}