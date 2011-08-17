using Cassette.UI;

namespace Cassette
{
    public interface ICassetteApplication
    {
        IFileSystem RootDirectory { get; }
        bool IsOutputOptimized { get; }

        IPageAssetManager<T> GetPageAssetManager<T>() where T : Module;
        string CreateModuleUrl(Module module);
        string CreateAssetUrl(Module module, IAsset asset);
        string CreateAbsoluteUrl(string path);
    }
}