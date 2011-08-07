using System;
using System.Collections.Generic;
using Cassette.Persistence;
using Moq;
using Should;
using Xunit;
using Cassette.ModuleProcessing;

namespace Cassette
{
    public class Initializer_Tests
    {
        public Initializer_Tests()
        {
            moduleFactory = new Mock<IModuleFactory<Module>>();
            reader = new Mock<IModuleContainerReader<Module>>();
            writer = new Mock<IModuleContainerWriter<Module>>();
            source = new Mock<IModuleContainerFactory<Module>>();
            pipeline = new Mock<IModuleProcessor<Module>>();
            cachedContainer = new Mock<IModuleContainer<Module>>();
            newContainer = new Mock<IModuleContainer<Module>>();

            reader.Setup(s => s.Load())
                  .Returns(cachedContainer.Object);
            source.Setup(s => s.CreateModuleContainer(moduleFactory.Object))
                  .Returns(newContainer.Object);
            newContainer.Setup(c => c.GetEnumerator())
                        .Returns(() => new List<Module>(new[] { new Module("", _ => null) }).GetEnumerator());

            initializer = new Initializer<Module>(moduleFactory.Object, reader.Object, writer.Object);
        }

        readonly Initializer<Module> initializer;
        readonly Mock<IModuleFactory<Module>> moduleFactory;
        readonly Mock<IModuleContainerReader<Module>> reader;
        readonly Mock<IModuleContainerWriter<Module>> writer;
        readonly Mock<IModuleContainerFactory<Module>> source;
        readonly Mock<IModuleProcessor<Module>> pipeline;
        readonly Mock<IModuleContainer<Module>> cachedContainer;
        readonly Mock<IModuleContainer<Module>> newContainer;

        [Fact]
        public void InitializeCreatesModulesFromSource()
        {
            initializer.Initialize(source.Object, pipeline.Object);

            source.Verify(s => s.CreateModuleContainer(moduleFactory.Object));
        }

        [Fact]
        public void GivenCacheIsUpToDate_ThenInitializeReturnsTheCacheContainer()
        {
            var now = DateTime.UtcNow;
            cachedContainer.SetupGet(c => c.LastWriteTime).Returns(now);
            newContainer.SetupGet(c => c.LastWriteTime).Returns(now);
            
            var result = initializer.Initialize(source.Object, pipeline.Object);

            result.ShouldBeSameAs(cachedContainer.Object);
        }

        [Fact]
        public void GivenCacheIsOutOfDate_ThenInitializeProcessesModulePipeline()
        {
            var now = DateTime.UtcNow;
            cachedContainer.SetupGet(c => c.LastWriteTime).Returns(now.AddDays(-1));
            newContainer.SetupGet(c => c.LastWriteTime).Returns(now);

            initializer.Initialize(source.Object, pipeline.Object);

            pipeline.Verify(p => p.Process(It.IsAny<Module>()));
        }

        [Fact]
        public void GivenCacheIsOutOfDate_ThenNewContainerIsSavedToStore()
        {
            var now = DateTime.UtcNow;
            cachedContainer.SetupGet(c => c.LastWriteTime).Returns(now.AddDays(-1));
            newContainer.SetupGet(c => c.LastWriteTime).Returns(now);

            initializer.Initialize(source.Object, pipeline.Object);

            writer.Verify(s => s.Save(It.IsAny<IModuleContainer<Module>>()));
        }
    }
}
