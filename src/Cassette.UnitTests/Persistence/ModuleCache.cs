using System;
using System.IO;
using System.Linq;
using Cassette.IO;
using Should;
using Xunit;
using Moq;

namespace Cassette.Persistence
{
    public class ModuleCache_SaveContainer_Tests
    {
        
    }

    public class ModuleCache_LoadContainerIfUpToDate_Tests
    {
        [Fact]
        public void GivenContainerFileDoesNotExist_WhenLoadContainerIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir));
                var sourceModules = new Module[0];

                IModuleContainer<Module> container;
                var result = cache.LoadContainerIfUpToDate(sourceModules, out container);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenContainerFileWithDifferentVersion_WhenLoadContainerIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION-1\" AssetCount=\"0\"></Container>"
                    );

                var cache = new ModuleCache<Module>("VERSION-2", new FileSystemDirectory(cacheDir));
                var sourceModules = new Module[0];

                IModuleContainer<Module> container;
                var result = cache.LoadContainerIfUpToDate(sourceModules, out container);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenContainerFileWithDifferentAssetCount_WhenLoadContainerIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\"></Container>"
                    );

                var moduleWithAsset = new Module("~/test");
                var asset = StubAsset();
                moduleWithAsset.Assets.Add(asset.Object);
                var sourceModules = new[] { moduleWithAsset };

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir));
                IModuleContainer<Module> container;
                var result = cache.LoadContainerIfUpToDate(sourceModules, out container);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenContainerFileIsOlderThanAnAssetFile_WhenLoadContainerIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\"></Container>"
                    );

                var moduleWithAsset = new Module("~/test");
                var asset = StubAsset();
                asset.Setup(a => a.SourceFile.LastWriteTimeUtc).Returns(DateTime.UtcNow);
                moduleWithAsset.Assets.Add(asset.Object);
                var sourceModules = new[] { moduleWithAsset };

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir));
                IModuleContainer<Module> container;
                var result = cache.LoadContainerIfUpToDate(sourceModules, out container);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenCacheIsUpToDate_WhenLoadContainerIfUpToDate_ThenReturnTrue()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\"><Module Path=\"~/test\" Hash=\"01\"/></Container>"
                    );
                File.WriteAllText(
                    Path.Combine(cacheDir, "test.module"),
                    "asset"
                    );

                var module = new Module("~/test");
                var sourceModules = new[] { module };

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir));
                IModuleContainer<Module> container;
                var result = cache.LoadContainerIfUpToDate(sourceModules, out container);

                result.ShouldBeTrue();
            }
        }

        [Fact]
        public void GivenCacheIsUpToDate_WhenLoadContainerIfUpToDate_ThenContainerHasModule()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\"><Module Path=\"~/test\" Hash=\"01\"/></Container>"
                    );
                File.WriteAllText(
                    Path.Combine(cacheDir, "test.module"),
                    "asset"
                    );

                var module = new Module("~/test");
                var sourceModules = new[] { module };

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir));
                IModuleContainer<Module> container;
                cache.LoadContainerIfUpToDate(sourceModules, out container);

                container.Modules.First().ShouldBeSameAs(module);
            }
        }

        [Fact]
        public void GivenCacheIsUpToDate_WhenLoadContainerIfUpToDate_ThenModuleAssetsReplacedWithCachedAsset()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"1\"><Module Path=\"~/test\" Hash=\"01\"/></Container>"
                    );
                File.WriteAllText(
                    Path.Combine(cacheDir, "test.module"),
                    "asset"
                    );
                var moduleWithAsset = new Module("~/test");
                var asset = StubAsset();
                moduleWithAsset.Assets.Add(asset.Object);
                var sourceModules = new[] { moduleWithAsset };

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir));
                IModuleContainer<Module> container;
                var result = cache.LoadContainerIfUpToDate(sourceModules, out container);

                result.ShouldBeTrue();
                moduleWithAsset.Assets[0].ShouldBeType<CachedAsset>();
            }
        }

        [Fact]
        public void GivenCacheIsUpToDate_WhenLoadContainerIfUpToDateWithZeroAssetModule_ThenReturnTrue()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\"><Module Path=\"~/test\" Hash=\"\"/></Container>"
                    );
                var module = new Module("~/test");
                var sourceModules = new[] { module };

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir));
                IModuleContainer<Module> container;
                var result = cache.LoadContainerIfUpToDate(sourceModules, out container);

                result.ShouldBeTrue();
            }
        }

        [Fact]
        public void GivenCacheMissingSecondFile_WhenLoadContainerIfUpToDate_ThenFirstModuleAssetsAreNotModified()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"2\"><Module Path=\"~/test1\" Hash=\"01\"/><Module Path=\"~/test2\" Hash=\"01\"/></Container>"
                    );
                File.WriteAllText(
                    Path.Combine(cacheDir, "test1.module"),
                    "asset"
                    );
                var module1 = new Module("~/test1");
                var module2 = new Module("~/test2");
                var asset1 = StubAsset();
                module1.Assets.Add(asset1.Object);
                var asset2 = StubAsset();
                module2.Assets.Add(asset2.Object);
                var sourceModules = new[] { module1, module2 };

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir));
                IModuleContainer<Module> container;
                var result = cache.LoadContainerIfUpToDate(sourceModules, out container);

                result.ShouldBeFalse();
                module1.Assets[0].ShouldNotBeType<CachedAsset>();
            }
        }

        [Fact]
        public void GivenContainerCacheHasModuleReferences_WhenLoadContainerIfUpToDate_ThenModuleReferencesAreSet()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\">" +
                    "<Module Path=\"~/test1\" Hash=\"01\">" +
                    "  <Reference Path=\"~/test2\"/>" +
                    "</Module>" +
                    "<Module Path=\"~/test2\" Hash=\"01\"/>" +
                    "</Container>"
                    );
                var module1 = new Module("~/test1");
                var module2 = new Module("~/test2");
                var sourceModules = new[] { module1, module2 };

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir));
                IModuleContainer<Module> container;
                cache.LoadContainerIfUpToDate(sourceModules, out container);

                module1.References.First().ShouldEqual("~/test2");
            }
        }

        Mock<IAsset> StubAsset()
        {
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.SourceFile.LastWriteTimeUtc)
                 .Returns(new DateTime(2000,1,1));

            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                .Callback<IAssetVisitor>(v => v.Visit(asset.Object));

            return asset;
        }
    }
}
