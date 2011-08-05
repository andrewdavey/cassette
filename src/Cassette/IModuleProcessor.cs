namespace Cassette
{
    public interface IModuleProcessor<in T>
        where T : Module
    {
        void Process(T module);
    }
}
