using System.Collections.Generic;

namespace Cassette.Persistence
{
    public interface IModuleCache<T>
        where T : Module
    {
        bool InitializeModulesFromCacheIfUpToDate(IEnumerable<T> unprocessedSourceModules);
        void SaveModuleContainer(IModuleContainer<T> moduleContainer);
    }
}