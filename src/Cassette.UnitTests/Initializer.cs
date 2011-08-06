using System.Collections.Generic;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class Initializer_Tests
    {
        public Initializer_Tests()
        {
            moduleFactory = new Mock<IModuleFactory<Module>>();
            store = new Mock<IModuleContainerStore<Module>>();
            source = new Mock<IModuleSource<Module>>();
            pipeline = new Mock<IModuleProcessor<Module>>();
            cachedContainer = new Mock<IModuleContainer<Module>>();

            store.Setup(s => s.Load())
                 .Returns(cachedContainer.Object);

            initializer = new Initializer<Module>(moduleFactory.Object, store.Object);
        }

        readonly Initializer<Module> initializer;
        readonly Mock<IModuleFactory<Module>> moduleFactory;
        readonly Mock<IModuleContainerStore<Module>> store;
        readonly Mock<IModuleSource<Module>> source;
        readonly Mock<IModuleProcessor<Module>> pipeline;
        readonly Mock<IModuleContainer<Module>> cachedContainer;

        [Fact]
        public void InitializeCreatesModulesFromSource()
        {
            initializer.Initialize(source.Object, pipeline.Object);

            source.Verify(s => s.CreateModules(moduleFactory.Object));
        }

        [Fact]
        public void GivenCacheIsUpToDate_ThenInitializeReturnsTheCacheContainer()
        {
            cachedContainer.Setup(c => c.IsUpToDate(It.IsAny<IEnumerable<Module>>()))
                           .Returns(true);
            
            var container = initializer.Initialize(source.Object, pipeline.Object);

            container.ShouldBeSameAs(cachedContainer.Object);
        }

        [Fact]
        public void GivenCacheIsOutOfDate_ThenInitializeProcessesModulePipeline()
        {
            source.Setup(s => s.CreateModules(moduleFactory.Object))
                  .Returns(new[] { new Module("c:\\") });

            initializer.Initialize(source.Object, pipeline.Object);

            pipeline.Verify(p => p.Process(It.IsAny<Module>()));
        }

        [Fact]
        public void GivenCacheIsOutOfDate_ThenNewContainerIsSavedToStore()
        {
            initializer.Initialize(source.Object, pipeline.Object);

            store.Verify(s => s.Save(It.IsAny<IModuleContainer<Module>>()));
        }
    }
}
