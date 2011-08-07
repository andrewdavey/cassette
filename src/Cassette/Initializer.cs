using Cassette.Persistence;
using Cassette.ModuleProcessing;

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

        public IModuleContainer<T> Initialize(IModuleContainerFactory<T> moduleSource, IModuleProcessor<T> moduleProcessorPipeline)
        {
            var currentContainer = moduleSource.CreateModuleContainer(moduleFactory);
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
                currentContainer.ValidateAndSortModules();
                moduleContainerStore.Save(currentContainer);
                return currentContainer;
            }
        }
    }
}
