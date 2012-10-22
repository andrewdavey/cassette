using System.Collections.Generic;

namespace Cassette.RequireJS
{
    public interface IAmdModuleCollection : IEnumerable<IAmdModule>
    {
        IAmdModule this[string path] { get; }
    }
}