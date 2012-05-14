namespace Cassette
{
    public interface ICompiler
    {
        CompileResult Compile(string source, CompileContext context);
    }
}