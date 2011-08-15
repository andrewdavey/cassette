using System.Collections.Generic;

namespace Cassette
{
    public interface IModuleSource<T>
        where T : Module
    {
        ModuleSourceResult<T> GetModules(IModuleFactory<T> moduleFactory, ICassetteApplication application);
    }
}