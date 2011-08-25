namespace Cassette.ModuleProcessing
{
    public class SortAssetsByDependency : IModuleProcessor<Module>
    {
        public void Process(Module module, ICassetteApplication application)
        {
            module.SortAssetsByDependency();
        }
    }
}