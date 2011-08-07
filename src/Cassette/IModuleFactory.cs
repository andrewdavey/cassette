namespace Cassette
{
    public interface IModuleFactory<out T>
        where T : Module
    {
        T CreateModule(string directory);
    }
}