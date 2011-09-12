using System;
using System.Collections.Generic;

namespace Cassette
{
    // TODO: Clean up these two interfaces by merging them?
    public interface IModuleContainer<out T> : ISearchableModuleContainer<T>
        where T : Module
    {
        IEnumerable<T> Modules { get; }
        IEnumerable<Module> IncludeReferencesAndSortModules(IEnumerable<Module> modules);
    }

    // This type-system trickery allows a List<ISearchableModuleContainer<Module>> to be searched
    // for any type of Module with the path. But a specific type of container module can still
    // return its strongly-typed modules.
    public interface ISearchableModuleContainer<out T> : IDisposable
        where T : Module
    {
        T FindModuleContainingPath(string path);
    }
}