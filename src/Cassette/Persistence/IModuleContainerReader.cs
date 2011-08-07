namespace Cassette.Persistence
{
    public interface IModuleContainerReader<T>
        where T : Module
    {
        IModuleContainer<T> Load();
    }
}
