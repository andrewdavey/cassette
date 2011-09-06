using System.Collections.Generic;

namespace Cassette
{
    interface IExternalModule
    {
        string Path { get; }
        IList<IAsset> Assets { get; }
    }
}
