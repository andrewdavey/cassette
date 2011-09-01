namespace Cassette
{
    public interface IModuleFactory<T>
        where T : Module
    {
        T CreateModule(string directory);
        T CreateExternalModule(string url);
        T CreateExternalModule(string name, ModuleDescriptor moduleDescriptor);
    }
}