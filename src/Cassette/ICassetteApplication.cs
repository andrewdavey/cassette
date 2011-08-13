using Cassette.UI;

namespace Cassette
{
    public interface ICassetteApplication
    {
        IFileSystem RootDirectory { get; }
        bool IsOutputOptimized { get; }
        string Version { get; }

        void AddModuleContainerFactory<T>(IModuleContainerFactory<T> moduleContainerFactory) where T : Module;
        IModuleCache<T> GetModuleCache<T>() where T : Module;
        IModuleContainer<T> GetModuleContainer<T>() where T : Module;
        IModuleFactory<T> GetModuleFactory<T>() where T : Module;
        IPageAssetManager<T> GetPageAssetManager<T>() where T : Module;
        string CreateModuleUrl(Module module);
        string CreateAssetUrl(Module module, IAsset asset);
        string CreateAbsoluteUrl(string path);
    }
}