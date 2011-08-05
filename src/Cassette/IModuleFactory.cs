namespace Cassette
{
    public interface IModuleFactory<T>
        where T : Module
    {
        T CreateModule(string directoryPath);
    }
}