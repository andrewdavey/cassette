using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Cassette.IO;
using Cassette.ModuleProcessing;
using Cassette.Scripts;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Persistence
{
    public class ModuleCache_SaveContainer_Tests
    {
        [Fact]
        public void WhenSaveContainer_ThenXmlManifestContainsVersion()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                cache.SaveModuleContainer(new ModuleContainer<Module>(new Module[0]));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("Version=\"VERSION\"");
            }
        }

        [Fact]
        public void GivenModuleHasConcatenatedAsset_WhenSaveContainer_ThenXmlManifestHasAssetCountOfChildAssets()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var module = new Module("~/test");
                var asset1 = StubAsset();
                var asset2 = StubAsset();
                module.Assets.Add(new ConcatenatedAsset(new[] { asset1.Object, asset2.Object }));

                cache.SaveModuleContainer(new ModuleContainer<Module>(new[] { module }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("AssetCount=\"2\"");
            }
        }

        [Fact]
        public void GivenModule_WhenSaveContainer_ThenXmlHasModuleElementWithPathAttribute()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var module = new Module("~/test");
                var asset1 = StubAsset();
                module.Assets.Add(asset1.Object);

                cache.SaveModuleContainer(new ModuleContainer<Module>(new[] { module }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("<Module Path=\"~/test\"");
            }
        }

        [Fact]
        public void GivenModuleWithReference_WhenSaveContainer_ThenXmlHasReferenceElement()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var module = new Module("~/test");
                module.AddReferences(new[] { "~/other" });
                
                cache.SaveModuleContainer(new ModuleContainer<Module>(new[] { module, new Module("~/other")  }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("<Reference Path=\"~/other\"");
            }
        }

        [Fact]
        public void GivenAssetWithReferenceToAnotherModule_WhenSaveContainer_ThenXmlHasReferenceElementWithReferencedModulePath()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var module1 = new Module("~/module-1");
                var module2 = new Module("~/module-2");
                var asset1 = StubAsset();
                var reference = new AssetReference("~/module-2", asset1.Object, -1, AssetReferenceType.DifferentModule);
                asset1.SetupGet(a => a.References)
                      .Returns(new[] { reference });
                module1.Assets.Add(asset1.Object);

                cache.SaveModuleContainer(new ModuleContainer<Module>(new[] { module1, module2 }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("<Reference Path=\"~/module-2\" />");
            }
        }

        [Fact]
        public void GivenAssetWithReferenceToAssetInAnotherModule_WhenSaveContainer_ThenXmlHasReferenceElementWithReferencedModulePath()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var module1 = new Module("~/module-1");
                var module2 = new Module("~/module-2");
                var asset1 = StubAsset();
                var reference = new AssetReference("~/module-2/asset2.js", asset1.Object, -1, AssetReferenceType.DifferentModule);
                asset1.SetupGet(a => a.References)
                      .Returns(new[] { reference });
                module1.Assets.Add(asset1.Object);

                module2.Assets.Add(StubAsset("~/module-2/asset2.js").Object);

                cache.SaveModuleContainer(new ModuleContainer<Module>(new[] { module1, module2 }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("<Reference Path=\"~/module-2\" />");
            }
        }

        [Fact]
        public void GivenAssetWithTwoReferencesToAssetsInAnotherModule_WhenSaveContainer_ThenXmlHasOneReferenceElementWithReferencedModulePath()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var module1 = new Module("~/module-1");
                var module2 = new Module("~/module-2");
                var asset1 = StubAsset();
                var reference1 = new AssetReference("~/module-2/asset2.js", asset1.Object, -1, AssetReferenceType.DifferentModule);
                var reference2 = new AssetReference("~/module-2/asset3.js", asset1.Object, -1, AssetReferenceType.DifferentModule);
                asset1.SetupGet(a => a.References)
                      .Returns(new[] { reference1, reference2 });
                module1.Assets.Add(asset1.Object);

                module2.Assets.Add(
                    new ConcatenatedAsset(new[]
                    {
                        StubAsset("~/module-2/asset2.js").Object, 
                        StubAsset("~/module-2/asset3.js").Object
                    })
                );

                cache.SaveModuleContainer(new ModuleContainer<Module>(new[] { module1, module2 }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                Regex.Matches(xml, Regex.Escape("<Reference Path=\"~/module-2\" />")).Count.ShouldEqual(1);
            }
        }

        [Fact]
        public void GivenAssetWithReferenceToAssetInSameModule_WhenSaveContainer_ThenXmlHasNoReferenceElements()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var module = new Module("~/module-1");
                var asset1 = StubAsset("~/module-1/asset1.js");
                var asset2 = StubAsset("~/module-1/asset2.js");
                var reference = new AssetReference("~/module-1/asset2.js", asset1.Object, -1, AssetReferenceType.SameModule);
                asset1.SetupGet(a => a.References)
                      .Returns(new[] { reference });
                module.Assets.Add(new ConcatenatedAsset(new[]
                {
                    asset1.Object,
                    asset2.Object
                }));

                cache.SaveModuleContainer(new ModuleContainer<Module>(new[] { module }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldNotContain("<Reference ");
            }
        }

        [Fact]
        public void GivenAssetWithReferenceToUrl_WhenSaveContainer_ThenXmlHasReferenceElementWithUrlAsPath()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var module = new Module("~/module-1");
                var asset = StubAsset();
                var reference = new AssetReference("http://test.com", asset.Object, -1, AssetReferenceType.Url);
                asset.SetupGet(a => a.References)
                     .Returns(new[] { reference });
                module.Assets.Add(asset.Object);

                cache.SaveModuleContainer(new ModuleContainer<Module>(new[] { module, new ExternalScriptModule("http://test.com"),  }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("<Reference Path=\"http://test.com\" />");
            }
        }

        [Fact]
        public void GivenAssetWithRawFilenameReference_WhenSaveContainer_ThenXmlHasFileElement()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var module = new Module("~/module-1");
                var asset = StubAsset();
                var reference = new AssetReference("~/images/test.png", asset.Object, -1, AssetReferenceType.RawFilename);
                asset.SetupGet(a => a.References)
                     .Returns(new[] { reference });
                module.Assets.Add(asset.Object);

                cache.SaveModuleContainer(new ModuleContainer<Module>(new[] { module }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("<File Path=\"~/images/test.png\" />");
            }                
        }

        [Fact]
        public void GivenModuleWithAsset_WhenSaveContainer_ThenModuleFileSavedWithContentsOfAsset()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var module = new Module("~/test");
                var asset = StubAsset();
                asset.Setup(a => a.OpenStream())
                     .Returns(() => "ASSET-DATA".AsStream());
                module.Assets.Add(asset.Object);

                cache.SaveModuleContainer(new ModuleContainer<Module>(new[] { module }));

                var savedData = File.ReadAllText(Path.Combine(cacheDir, "test.module"));
                savedData.ShouldEqual("ASSET-DATA");
            }
        }

        [Fact]
        public void GivenModuleWithNoAssets_WhenSaveContainer_ThenNoModuleFileCreated()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var module = new Module("~/test");

                cache.SaveModuleContainer(new ModuleContainer<Module>(new[] { module }));

                File.Exists(Path.Combine(cacheDir, "test.module")).ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenModuleWithTwoAssets_WhenSaveContainer_ThenExceptionThrownBecauseAssetsMustBeConcatenatedBeforeCaching()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var module = new Module("~/test");
                module.Assets.Add(StubAsset().Object);
                module.Assets.Add(StubAsset().Object);

                Assert.Throws<InvalidOperationException>(
                    () => cache.SaveModuleContainer(new ModuleContainer<Module>(new[] { module }))
                );
            }
        }

        Mock<IAsset> StubAsset(string path = null)
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns(path);
            asset.SetupGet(a => a.Hash).Returns(new byte[0]);
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            asset.Setup(a => a.Accept(It.IsAny<IAssetVisitor>()))
                 .Callback<IAssetVisitor>(v => v.Visit(asset.Object));
            return asset;
        }
    }

    public class ModuleCache_InitializeModulesFromCacheIfUpToDate_Tests
    {
        [Fact]
        public void GivenContainerFileDoesNotExist_WhenInitializeModulesFromCacheIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var sourceModules = new Module[0];

                var result = cache.InitializeModulesFromCacheIfUpToDate(sourceModules);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenContainerFileWithDifferentVersion_WhenInitializeModulesFromCacheIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION-1\" AssetCount=\"0\"></Container>"
                    );

                var cache = new ModuleCache<Module>("VERSION-2", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var sourceModules = new Module[0];

                var result = cache.InitializeModulesFromCacheIfUpToDate(sourceModules);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenContainerFileWithDifferentAssetCount_WhenInitializeModulesFromCacheIfUpToDate_ThenReturnFalse()
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

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var result = cache.InitializeModulesFromCacheIfUpToDate(sourceModules);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenContainerFileIsOlderThanAnAssetFile_WhenInitializeModulesFromCacheIfUpToDate_ThenReturnFalse()
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

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var result = cache.InitializeModulesFromCacheIfUpToDate(sourceModules);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenCacheIsUpToDate_WhenInitializeModulesFromCacheIfUpToDate_ThenReturnTrue()
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

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var result = cache.InitializeModulesFromCacheIfUpToDate(sourceModules);

                result.ShouldBeTrue();
            }
        }

        [Fact]
        public void GivenCacheIsUpToDate_WhenInitializeModulesFromCacheIfUpToDate_ThenModuleAssetsReplacedWithCachedAsset()
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

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var result = cache.InitializeModulesFromCacheIfUpToDate(sourceModules);

                result.ShouldBeTrue();
                moduleWithAsset.Assets[0].OpenStream().ReadToEnd().ShouldEqual("asset");
            }
        }

        [Fact]
        public void GivenCacheIsUpToDate_WhenInitializeModulesFromCacheIfUpToDateWithZeroAssetModule_ThenReturnTrue()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\"><Module Path=\"~/test\" Hash=\"\"/></Container>"
                    );
                var module = new Module("~/test");
                var sourceModules = new[] { module };

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var result = cache.InitializeModulesFromCacheIfUpToDate(sourceModules);

                result.ShouldBeTrue();
            }
        }

        [Fact]
        public void GivenCacheMissingSecondFile_WhenInitializeModulesFromCacheIfUpToDate_ThenFirstModuleAssetsAreNotModified()
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

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var result = cache.InitializeModulesFromCacheIfUpToDate(sourceModules);

                result.ShouldBeFalse();
                module1.Assets[0].ShouldNotBeType<CachedAsset>();
            }
        }

        [Fact]
        public void GivenContainerCacheHasModuleReferences_WhenInitializeModulesFromCacheIfUpToDate_ThenModuleReferencesAreSet()
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

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                cache.InitializeModulesFromCacheIfUpToDate(sourceModules);

                module1.References.First().ShouldEqual("~/test2");
            }
        }

        [Fact]
        public void GivenContainerCacheHasFileReferenceThatIsModifiedSinceCacheWrite_WhenInitializeModulesFromCacheIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            using (var sourceDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\">" +
                    "<Module Path=\"~/test\" Hash=\"01\">" +
                    "  <File Path=\"~/file.png\"/>" +
                    "</Module>" +
                    "</Container>"
                );
                Thread.Sleep(10); // Brief pause, otherwise the file write times are the same.
                File.WriteAllText(Path.Combine(sourceDir, "file.png"), "");
                var module = new Module("~/test");
                var sourceModules = new[] { module };

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), new FileSystemDirectory(sourceDir));
                var result = cache.InitializeModulesFromCacheIfUpToDate(sourceModules);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenContainerCacheHasFileReferenceToMissingFile_WhenInitializeModulesFromCacheIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            using (var sourceDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\">" +
                    "<Module Path=\"~/test\" Hash=\"01\">" +
                    "  <File Path=\"~/file.png\"/>" +
                    "</Module>" +
                    "</Container>"
                );
                var module = new Module("~/test");
                var sourceModules = new[] { module };

                var cache = new ModuleCache<Module>("VERSION", new FileSystemDirectory(cacheDir), new FileSystemDirectory(sourceDir));
                var result = cache.InitializeModulesFromCacheIfUpToDate(sourceModules);

                result.ShouldBeFalse();
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
