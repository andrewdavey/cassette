namespace Cassette.Configuration
{
// ReSharper disable UnusedTypeParameter
// The T type parameter makes it easy to distinguish between file search modifiers for the different type of bundles
    public interface IFileSearchModifier<T>
        where T : Bundle
// ReSharper restore UnusedTypeParameter
    {
        void Modify(FileSearch fileSearch);
    }
}