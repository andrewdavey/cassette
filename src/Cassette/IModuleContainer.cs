using System.Collections.Generic;

namespace Cassette
{
    public interface IModuleContainer<T>
        where T : Module
    {
        bool IsUpToDate(IEnumerable<T> currentModules);
    }
}
