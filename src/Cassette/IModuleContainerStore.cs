namespace Cassette
{
    public interface IModuleContainerStore<T>
        where T : Module
    {
        void Save(IModuleContainer<T> moduleContainer);
        IModuleContainer<T> Load();
    }
}
