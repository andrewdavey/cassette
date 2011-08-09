using System;
using System.IO;
using System.Linq;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;
using Cassette.Persistence;
using Cassette.ModuleProcessing;

namespace Cassette.IntegrationTests
{
    public class ModuleCache_Tests : IDisposable
    {
        public ModuleCache_Tests()
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

        readonly ICassetteApplication application;
        readonly DateTime now;
        readonly string root;
        readonly Func<string, string> getFullPath;

        [Fact]
        public void CanRoundTripInStoreModuleContainerWithRootedModule()
        {
            var cacheDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(cacheDirectory);
            try
            {
                var fileSystem = new FileSystem(cacheDirectory);
                var cache = new ModuleCache<ScriptModule>(fileSystem, new ScriptModuleFactory(getFullPath));

                var container = StubModuleContainerWithRootedModule();
                ConcatentateAssets(container);
                cache.SaveModuleContainer(container);
                var loadedContainer = cache.LoadModuleContainer();

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
                .CreateModuleContainer();
        }

        [Fact]
        public void CanRoundTripInStoreModuleContainerWithSubDirectoryModule()
        {
            var cacheDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(cacheDirectory);
            try
            {
                var fileSystem = new FileSystem(cacheDirectory);
                var writer = new ModuleContainerWriter<ScriptModule>(fileSystem);
                var reader = new ModuleContainerReader<ScriptModule>(fileSystem, new ScriptModuleFactory(getFullPath));
                
                var container = StubModuleContainerWithSubDirectoryModule();
                ConcatentateAssets(container);
                writer.Save(container);
                var loadedContainer = reader.Load(fileSystem);

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
                .CreateModuleContainer(new ScriptModuleFactory(getFullPath));
        }

        public void Dispose()
        {
            Directory.Delete(root, true);
        }
    }
}
