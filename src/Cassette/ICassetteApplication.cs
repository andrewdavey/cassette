using Cassette.UI;

namespace Cassette
{
    public interface ICassetteApplication
    {
        IFileSystem RootDirectory { get; }
        bool IsOutputOptimized { get; }
        string Version { get; }

        void Add<T>(IModuleSource<T> moduleSource) where T : Module;
        IPageAssetManager<T> GetPageAssetManager<T>() where T : Module;
        string CreateModuleUrl(Module module);
        string CreateAssetUrl(Module module, IAsset asset);
        string CreateAbsoluteUrl(string path);
    }
}