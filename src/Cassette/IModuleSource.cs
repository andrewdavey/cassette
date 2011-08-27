using System.Collections.Generic;

namespace Cassette
{
    public interface IModuleSource<T>
        where T : Module
    {
        IEnumerable<T> GetModules(IModuleFactory<T> moduleFactory, ICassetteApplication application);
    }
}