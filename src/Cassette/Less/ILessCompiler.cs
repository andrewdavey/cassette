namespace Cassette.Less
{
    public interface ILessCompiler
    {
        string CompileFile(string lessSource);
    }
}
