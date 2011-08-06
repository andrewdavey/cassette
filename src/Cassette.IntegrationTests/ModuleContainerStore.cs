using System;
using System.IO;
using System.Linq;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.IntegrationTests
{
    public class ModuleContainerStore_Tests
    {
        readonly DateTime now = DateTime.UtcNow;

        [Fact]
        public void CanRoundTripInStoreModuleContainerWithRootedModule()
        {
            var cacheDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(cacheDirectory);
            try
            {
                var fileSystem = new FileSystem(cacheDirectory);
                var store = new ModuleContainerStore<ScriptModule>(fileSystem, new ScriptModuleFactory());

                var container = StubModuleContainerWithRootedModule();
                store.Save(container);
                var loadedContainer = store.Load();

                loadedContainer.LastWriteTime.ShouldEqual(now);
                loadedContainer.First().Assets.Count.ShouldEqual(1);
                loadedContainer.First().Assets[0].OpenStream().ReadAsString().ShouldEqual("asset-content");
            }
            finally
            {
                Directory.Delete(cacheDirectory, true);
            }
        }

        ModuleContainer<ScriptModule> StubModuleContainerWithRootedModule()
        {
            var modules = new[] {
                new ScriptModule("c:\\test")
            };
            var asset1 = new Mock<IAsset>();
            asset1.SetupGet(a => a.SourceFilename).Returns("asset.js");
            asset1.Setup(a => a.OpenStream()).Returns("asset-content".AsStream());
            modules[0].Assets.Add(asset1.Object);
            return new ModuleContainer<ScriptModule>(modules, now, "c:\\test");
        }

        [Fact]
        public void CanRoundTripInStoreModuleContainerWithSubDirectoryModule()
        {
            var cacheDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(cacheDirectory);
            try
            {
                var fileSystem = new FileSystem(cacheDirectory);
                var store = new ModuleContainerStore<ScriptModule>(fileSystem, new ScriptModuleFactory());

                var container = StubModuleContainerWithSubDirectoryModule();
                store.Save(container);
                var loadedContainer = store.Load();

                loadedContainer.LastWriteTime.ShouldEqual(now);
                loadedContainer.First().Assets.Count.ShouldEqual(1);
                loadedContainer.First().Assets[0].OpenStream().ReadAsString().ShouldEqual("asset-content");
            }
            finally
            {
                Directory.Delete(cacheDirectory, true);
            }
        }

        ModuleContainer<ScriptModule> StubModuleContainerWithSubDirectoryModule()
        {
            var modules = new[] {
                new ScriptModule("c:\\test\\module-a")
            };
            var asset1 = new Mock<IAsset>();
            asset1.SetupGet(a => a.SourceFilename).Returns("asset.js");
            asset1.Setup(a => a.OpenStream()).Returns("asset-content".AsStream());
            modules[0].Assets.Add(asset1.Object);
            return new ModuleContainer<ScriptModule>(modules, now, "c:\\test");
        }
    }
}
