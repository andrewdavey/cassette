using System;
using System.Collections.Generic;

namespace Cassette
{
    public interface IModuleContainer<T> : IModuleContainer
        where T : Module
    {
        IEnumerable<T> Modules { get; }
        T FindModuleByPath(string path);
        IEnumerable<T> ConcatDependencies(T module);
        IEnumerable<T> SortModules(IEnumerable<T> modules);
    }

    public interface IModuleContainer
    {
        IAsset FindAssetByPath(string path);
    }
}