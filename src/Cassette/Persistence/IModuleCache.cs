using System.Collections.Generic;

namespace Cassette.Persistence
{
    public interface IModuleCache<T>
        where T : Module
    {
        bool LoadContainerIfUpToDate(IEnumerable<T> unprocessedSourceModules, out IModuleContainer<T> container);
        IModuleContainer<T> SaveModuleContainer(IEnumerable<T> modules);
    }
}