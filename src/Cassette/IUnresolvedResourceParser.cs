using System.IO;

namespace Cassette
{
    public interface IUnresolvedResourceParser
    {
        UnresolvedResource Parse(Stream source, string sourcePath);
    }
}
