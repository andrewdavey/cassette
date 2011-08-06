using System;
using System.Collections.Generic;

namespace Cassette
{
    public interface IModuleContainer<T> : IEnumerable<T>
        where T : Module
    {
        void ValidateAndSortModules();
        DateTime LastWriteTime { get; }
        string RootDirectory { get; }
    }
}
