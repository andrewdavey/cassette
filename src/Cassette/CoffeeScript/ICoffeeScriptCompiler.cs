namespace Cassette.CoffeeScript
{
    public interface ICoffeeScriptCompiler
    {
        string Compile(string coffeeScriptSource, string filename);
    }
}