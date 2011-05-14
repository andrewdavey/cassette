using System.IO;

namespace Knapsack
{
    public interface IUnresolvedResourceParser
    {
        UnresolvedResource Parse(Stream source, string sourcePath);
    }
}
