using System.Collections.Generic;

namespace Cassette.Persistence
{
    public interface IModuleCache<T>
        where T : Module
    {
        bool LoadContainerIfUpToDate(IEnumerable<T> externalModules, int expectedAssetCount, string version, IFileSystem sourceFileSystem, out IModuleContainer<T> container);
        IModuleContainer<T> SaveModuleContainer(IEnumerable<T> modules, string version);
    }
}