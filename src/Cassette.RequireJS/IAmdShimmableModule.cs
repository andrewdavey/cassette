using System.Collections.Generic;

namespace Cassette.RequireJS
{
    internal interface IAmdShimmableModule : IAmdModule
    {
        bool Shim { get; set; }
        string ShimExports { get; set; }
        IEnumerable<string> DependencyPaths { get; }
    }
}