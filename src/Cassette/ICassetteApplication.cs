using Cassette.UI;

namespace Cassette
{
    public interface ICassetteApplication
    {
        IFileSystem RootDirectory { get; }
        bool IsOutputOptimized { get; }
        IUrlGenerator UrlGenerator { get; }

        IPageAssetManager<T> GetPageAssetManager<T>() where T : Module;
        string CreateModuleUrl(Module module);
        string CreateAbsoluteUrl(string path);
    }
}