namespace Cassette.Configuration
{
    public interface IFileSearchModifier<T>
    {
        void Modify(FileSearch fileSearch);
    }
}