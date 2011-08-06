using System;
using System.Collections.Generic;

namespace Cassette
{
    public interface IModuleSource<T>
        where T : Module
    {
        IModuleContainer<T> CreateModules(IModuleFactory<T> moduleFactory);
    }
}
