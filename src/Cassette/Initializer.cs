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
            var currentContainer = moduleSource.CreateModules(moduleFactory);
            var cachedModuleContainer = moduleContainerStore.Load();
            if (cachedModuleContainer.LastWriteTime == currentContainer.LastWriteTime)
            {
                return cachedModuleContainer;
            }
            else
            {
                foreach (var module in currentContainer)
                {
                    moduleProcessorPipeline.Process(module);
                }
                moduleContainerStore.Save(currentContainer);
                return currentContainer;
            }
        }
    }
}
