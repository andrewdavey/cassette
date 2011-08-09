using System;

namespace Cassette
{
    public interface ICassetteApplication
    {
        void AddModuleContainerFactory<T>(IModuleContainerFactory<T> moduleContainerFactory) where T : Module;
        IModuleCache<T> GetModuleCache<T>() where T : Module;
        IModuleContainer<T> GetModuleContainer<T>() where T : Module;
        IModuleFactory<T> GetModuleFactory<T>() where T : Module;
        IFileSystem RootDirectory { get; }
    }
}
