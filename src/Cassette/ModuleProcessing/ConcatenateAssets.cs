namespace Cassette.ModuleProcessing
{
    public class ConcatenateAssets : IModuleProcessor<Module>
    {
        public void Process(Module module, ICassetteApplication application)
        {
            module.ConcatenateAssets();
        }
    }
}