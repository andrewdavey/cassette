using Cassette.Persistence;
using Cassette.ModuleProcessing;

namespace Cassette
{
    public class Initializer<T>
        where T : Module
    {
        public Initializer(IModuleFactory<T> moduleFactory, IModuleContainerReader<T> moduleContainerReader, IModuleContainerWriter<T> moduleContainerWriter)
        {
            this.moduleFactory = moduleFactory;
            this.moduleContainerReader = moduleContainerReader;
            this.moduleContainerWriter = moduleContainerWriter;
        }

        readonly IModuleFactory<T> moduleFactory;
        readonly IModuleContainerReader<T> moduleContainerReader;
        readonly IModuleContainerWriter<T> moduleContainerWriter;

        public IModuleContainer<T> Initialize(IModuleContainerFactory<T> moduleSource, IModuleProcessor<T> moduleProcessorPipeline)
        {
            var currentContainer = moduleSource.CreateModuleContainer(moduleFactory);
            var cachedModuleContainer = moduleContainerReader.Load();
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
                moduleContainerWriter.Save(currentContainer);
                return currentContainer;
            }
        }
    }
}
