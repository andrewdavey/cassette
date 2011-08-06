using System;
using System.Collections.Generic;

namespace Cassette
{
    public interface IModuleContainer<T> : IEnumerable<T>
        where T : Module
    {
        DateTime LastWriteTime { get; }
    }
}
