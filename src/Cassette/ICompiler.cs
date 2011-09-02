using Cassette.IO;

namespace Cassette
{
    public interface ICompiler
    {
        string Compile(string source, string filename, IDirectory currentDirectory);
    }
}