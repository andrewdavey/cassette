using System.Collections.Generic;
using Cassette.IO;

namespace Cassette.Persistence
{
    public interface IModuleCache<T>
        where T : Module
    {
        bool LoadContainerIfUpToDate(IEnumerable<T> unprocessedSourceModules, string version, IDirectory sourceFileSystem, out IModuleContainer<T> container);
        IModuleContainer<T> SaveModuleContainer(IEnumerable<T> modules, string version);
    }
}