using System.Collections.Generic;

namespace Cassette
{
    public interface IModuleContainer<T> : ISearchableModuleContainer<T>
        where T : Module
    {
        IEnumerable<T> Modules { get; }
        IEnumerable<T> ConcatDependencies(T module);
        IEnumerable<T> SortModules(IEnumerable<T> modules);
    }

    // This type-system trickery allows a List<ISearchableModuleContainer<Module>> to be searched
    // for any type of Module with the path. But a specific type of container module can still
    // return its strongly-typed modules.
    public interface ISearchableModuleContainer<out T>
        where T : Module
    {
        T FindModuleContainingPath(string path);
    }
}