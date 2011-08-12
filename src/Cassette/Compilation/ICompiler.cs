namespace Cassette.Compilation
{
    public interface ICompiler
    {
        string Compile(string source, string filename, IFileSystem currentDirectory);
    }
}