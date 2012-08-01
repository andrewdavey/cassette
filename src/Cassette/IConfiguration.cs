namespace Cassette
{
    public interface IConfiguration<in T>
    {
        void Configure(T configurable);
    }
}