namespace Cassette.ModuleProcessing
{
    public interface IModuleProcessor<in T>
        where T : Module
    {
        void Process(T module, ICassetteApplication application);
    }
}
