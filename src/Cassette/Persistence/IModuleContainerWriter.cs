namespace Cassette.Persistence
{
    public interface IModuleContainerWriter<T>
        where T : Module
    {
        void Save(IModuleContainer<T> moduleContainer);
    }
}
