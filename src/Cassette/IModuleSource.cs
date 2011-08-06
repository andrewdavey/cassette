using System;
using System.Collections.Generic;

namespace Cassette
{
    public interface IModuleSource<T>
        where T : Module
    {
        IEnumerable<T> CreateModules(IModuleFactory<T> moduleFactory);
    }
}
