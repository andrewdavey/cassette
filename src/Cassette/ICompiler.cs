using Cassette.IO;

namespace Cassette
{
    public interface ICompiler
    {
        string Compile(string source, IFile sourceFile);
    }
}