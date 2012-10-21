namespace Cassette.RequireJS
{
    public interface IAmdModuleCollection
    {
        AmdModule this[string path] { get; }
    }
}