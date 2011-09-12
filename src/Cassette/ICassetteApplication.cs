using System;
using Cassette.IO;
using Cassette.UI;

namespace Cassette
{
    public interface ICassetteApplication : IDisposable
    {
        IDirectory RootDirectory { get; }
        bool IsOutputOptimized { get; set; }
        IUrlGenerator UrlGenerator { get; set; }

        IPageAssetManager<T> GetPageAssetManager<T>() where T : Module;
    }
}