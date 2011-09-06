using System.IO;
using Cassette.IO;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette.Persistence
{
    public class ModuleCache_Tests
    {
        [Fact]
        public void GivenExternalModuleHasNoFallbackAssets_WhenLoadContainerIfUpToDate_ThenItReturnsTrue()
        {
            using (var sourceDir = new TempDirectory())
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<ScriptModule>(new FileSystemDirectory(cacheDir), new ScriptModuleFactory());
                cache.SaveModuleContainer(new[] { new ExternalScriptModule("test", "http://test.com/") }, "version");

                // Load from cache. New instance of module to simulate real world use.
                IModuleContainer<ScriptModule> container;
                var isUpToDate = cache.LoadContainerIfUpToDate(
                    new[] { new ExternalScriptModule("test", "http://test.com/") },
                    "version",
                    new FileSystemDirectory(sourceDir),
                    out container
                );

                isUpToDate.ShouldBeTrue();
            }
        }

        [Fact]
        public void GivenExternalModuleWithFallbackAssets_WhenLoadContainerIfUpToDate_ThenModuleAssetsIsModifiedToContainCachedAsset()
        {
            using (var sourceDir = new TempDirectory())
            using (var cacheDir = new TempDirectory())
            {
                WriteCache(cacheDir, sourceDir);

                var externalModule = CreateExternalModuleWithFallbackAssets(sourceDir);

                var cache = new ModuleCache<ScriptModule>(new FileSystemDirectory(cacheDir), new ScriptModuleFactory());
                IModuleContainer<ScriptModule> container;
                var isUpToDate = cache.LoadContainerIfUpToDate(new[] { externalModule }, "version", new FileSystemDirectory(sourceDir), out container);

                isUpToDate.ShouldBeTrue();

                // Check that the external module's Asset is overwritten with the cached copy.
                externalModule.Assets.Count.ShouldEqual(1);
                externalModule.Assets[0].ShouldBeType<CachedAsset>();
            }
        }

        void WriteCache(TempDirectory cacheDir, TempDirectory sourceDir)
        {
            // External module with two source assets.
            Directory.CreateDirectory(Path.Combine(sourceDir, "test"));
            File.WriteAllText(Path.Combine(sourceDir, "test", "asset1.js"), "");
            File.WriteAllText(Path.Combine(sourceDir, "test", "asset2.js"), "");
            var module = CreateExternalModuleWithFallbackAssets(sourceDir);

            // Process the module to concatenate the assets.
            // This is so we can cache the module to disk.
            var application = new Mock<ICassetteApplication>();
            application.SetupGet(a => a.IsOutputOptimized).Returns(true);
            module.Process(application.Object);

            // Create the cache on disk.
            var cache = new ModuleCache<ScriptModule>(new FileSystemDirectory(cacheDir), new ScriptModuleFactory());
            cache.SaveModuleContainer(new[] { module }, "version");
        }

        ExternalScriptModule CreateExternalModuleWithFallbackAssets(TempDirectory sourceDir)
        {
            var module = new ExternalScriptModule("~/test", "http://test.com/");
            var testDir = new FileSystemDirectory(sourceDir).NavigateTo("test", false);
            var asset1 = new Asset(
                "~/test/asset1.js",
                module,
                new FileSystemFile(Path.Combine(sourceDir, "test", "asset1.js"), testDir)
            );
            var asset2 = new Asset(
                "~/test/asset1.js",
                module,
                new FileSystemFile(Path.Combine(sourceDir, "test", "asset2.js"), testDir)
            );
            module.AddFallback("", new[] { asset1, asset2 });
            return module;
        }
    }
}
