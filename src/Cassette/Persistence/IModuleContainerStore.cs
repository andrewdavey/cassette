namespace Cassette.Persistence
{
    public interface IModuleContainerStore<T>
        where T : Module
    {
        void Save(IModuleContainer<T> moduleContainer);
        IModuleContainer<T> Load();
    }
}
