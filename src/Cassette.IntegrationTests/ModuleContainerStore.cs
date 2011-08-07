using System;
using System.IO;
using System.Linq;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;
using Cassette.Persistence;

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

            getFullPath = (path) => Path.Combine(root, "scripts", path);
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
                ConcatentateAssets(container);
                store.Save(container);
                var loadedContainer = store.Load();

                loadedContainer.LastWriteTime.ShouldEqual(now);
                loadedContainer.First().Assets.Count.ShouldEqual(1);
                loadedContainer.First().ContainsPath("module-a\\test-1.js").ShouldBeTrue();
                loadedContainer.First().ContainsPath("module-a\\test-2.js").ShouldBeTrue();
                loadedContainer.First().ContainsPath("module-b\\test-3.js").ShouldBeTrue();
                loadedContainer.First().Assets[0].OpenStream().ReadAsString()
                    .ShouldEqual(string.Join(Environment.NewLine, "test-1", "test-2", "test-3"));
            }
            finally
            {
                Directory.Delete(cacheDirectory, true);
            }
        }

        void ConcatentateAssets(IModuleContainer<ScriptModule> container)
        {
            foreach (var module in container)
            {
                new ConcatenateAssets().Process(module);
            }
        }

        IModuleContainer<ScriptModule> StubModuleContainerWithRootedModule()
        {
            return new ModuleSource<ScriptModule>(Path.Combine(root, "scripts"), "*.js")
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
                ConcatentateAssets(container);
                store.Save(container);
                var loadedContainer = store.Load();

                loadedContainer.LastWriteTime.ShouldEqual(now);
                loadedContainer.First().Assets.Count.ShouldEqual(1);
                loadedContainer.First().Assets[0].OpenStream().ReadAsString()
                    .ShouldEqual("test-1" + Environment.NewLine + "test-2");
            }
            finally
            {
                Directory.Delete(cacheDirectory, true);
            }
        }

        IModuleContainer<ScriptModule> StubModuleContainerWithSubDirectoryModule()
        {
            return new ModuleSource<ScriptModule>(Path.Combine(root, "scripts"), "*.js")
                .AddEachSubDirectory()
                .CreateModules(new ScriptModuleFactory(getFullPath));
        }

        public void Dispose()
        {
            Directory.Delete(root, true);
        }
    }
}
