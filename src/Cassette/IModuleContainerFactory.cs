namespace Cassette
{
    public interface IModuleContainerFactory<T>
        where T : Module
    {
        IModuleContainer<T> CreateModuleContainer(IModuleFactory<T> moduleFactory);
    }
}