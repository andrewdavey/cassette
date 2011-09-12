using System;
using System.Collections.Generic;

namespace Cassette
{
    public interface IModuleContainer<out T> : IDisposable
        where T : Module
    {
        IEnumerable<T> Modules { get; }
        T FindModuleContainingPath(string path);
        IEnumerable<Module> IncludeReferencesAndSortModules(IEnumerable<Module> modules);
    }
}