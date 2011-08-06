namespace Cassette
{
    public class Initializer<T>
        where T : Module
    {
        public Initializer(IModuleFactory<T> moduleFactory, IModuleContainerStore<T> moduleContainerStore)
        {
            this.moduleFactory = moduleFactory;
            this.moduleContainerStore = moduleContainerStore;
        }

        readonly IModuleFactory<T> moduleFactory;
        readonly IModuleContainerStore<T> moduleContainerStore;

        public IModuleContainer<T> Initialize(IModuleSource<T> moduleSource, IModuleProcessor<T> moduleProcessorPipeline)
        {
            var modules = moduleSource.CreateModules(moduleFactory);
            var cachedModuleContainer = moduleContainerStore.Load();
            if (cachedModuleContainer.IsUpToDate(modules))
            {
                return cachedModuleContainer;
            }
            else
            {
                foreach (var module in modules)
                {
                    moduleProcessorPipeline.Process(module);
                }
                var container = new ModuleContainer<T>(modules);
                moduleContainerStore.Save(container);
                return container;
            }
        }
    }
}
