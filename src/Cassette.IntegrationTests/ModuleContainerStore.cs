using System;
using System.IO;
using System.Linq;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.IntegrationTests
{
    public class ModuleContainerStore_Tests : IDisposable
    {
        public ModuleContainerStore_Tests()
        {
            // Create a basic set of directories and files.
            root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(Path.Combine(root, "scripts"));
                Directory.CreateDirectory(Path.Combine(root, "scripts", "module-a"));
                    File.WriteAllText(Path.Combine(root, "scripts", "module-a", "test-1.js"), "test-1");
                    File.WriteAllText(Path.Combine(root, "scripts", "module-a", "test-2.js"), "test-2");
                Directory.CreateDirectory(Path.Combine(root, "scripts", "module-b"));
                    File.WriteAllText(Path.Combine(root, "scripts", "module-b", "test-3.js"), "test-3");

            now = File.GetLastWriteTimeUtc(Path.Combine(root, "scripts", "module-b", "test-3.js"));

            getFullPath = (path) => Path.Combine(root, path);
        }

        readonly DateTime now;
        readonly string root;
        private Func<string, string> getFullPath;

        [Fact]
        public void CanRoundTripInStoreModuleContainerWithRootedModule()
        {
            var cacheDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(cacheDirectory);
            try
            {
                var fileSystem = new FileSystem(cacheDirectory);
                var store = new ModuleContainerStore<ScriptModule>(fileSystem, new ScriptModuleFactory(getFullPath));

                var container = StubModuleContainerWithRootedModule();
                store.Save(container);
                var loadedContainer = store.Load();

                loadedContainer.LastWriteTime.ShouldEqual(now);
                loadedContainer.First().Assets.Count.ShouldEqual(1);
                loadedContainer.First().ContainsPath("scripts\\module-b\\test-3.js").ShouldBeTrue();
                loadedContainer.First().Assets[0].OpenStream().ReadAsString().ShouldEqual("test-3");
            }
            finally
            {
                Directory.Delete(cacheDirectory, true);
            }
        }

        IModuleContainer<ScriptModule> StubModuleContainerWithRootedModule()
        {
            return new ModuleSource<ScriptModule>(root, "*.js")
                .AsSingleModule()
                .CreateModules(new ScriptModuleFactory(getFullPath));
        }

        [Fact]
        public void CanRoundTripInStoreModuleContainerWithSubDirectoryModule()
        {
            var cacheDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(cacheDirectory);
            try
            {
                var fileSystem = new FileSystem(cacheDirectory);
                var store = new ModuleContainerStore<ScriptModule>(fileSystem, new ScriptModuleFactory(getFullPath));

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

        IModuleContainer<ScriptModule> StubModuleContainerWithSubDirectoryModule()
        {
            return new ModuleSource<ScriptModule>(root, "*.js")
                .AddEachSubDirectory()
                .CreateModules(new ScriptModuleFactory(getFullPath));
        }

        public void Dispose()
        {
            Directory.Delete(root, true);
        }
    }
}
