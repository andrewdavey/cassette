using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Persistence
{
    public class BundleCache_SaveContainer_Tests
    {
        [Fact]
        public void WhenSaveContainer_ThenXmlManifestContainsVersion()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                cache.SaveBundleContainer(new BundleContainer(new TestableBundle[0]));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("Version=\"VERSION\"");
            }
        }

        [Fact]
        public void GivenBundleHasConcatenatedAsset_WhenSaveContainer_ThenXmlManifestHasAssetCountOfChildAssets()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle = new TestableBundle("~/test");
                var asset1 = StubAsset();
                var asset2 = StubAsset();
                bundle.Assets.Add(new ConcatenatedAsset(new[] { asset1.Object, asset2.Object }));

                cache.SaveBundleContainer(new BundleContainer(new[] { bundle }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("AssetCount=\"2\"");
            }
        }

        [Fact]
        public void GivenBundle_WhenSaveContainer_ThenXmlHasBundleElementWithPathAttribute()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle = new TestableBundle("~/test");
                var asset1 = StubAsset();
                bundle.Assets.Add(asset1.Object);

                cache.SaveBundleContainer(new BundleContainer(new[] { bundle }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("<Bundle Path=\"~/test\"");
            }
        }

        [Fact]
        public void GivenBundle_WhenSaveContainer_ThenXmlHasBundleElementWithHashAttribute()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle = new TestableBundle("~/test");
                var asset1 = StubAsset();
                bundle.Assets.Add(asset1.Object);

                cache.SaveBundleContainer(new BundleContainer(new[] { bundle }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("Hash=\"010203\"");
            }
        }

        [Fact]
        public void GivenBundleWithReference_WhenSaveContainer_ThenXmlHasReferenceElement()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle = new TestableBundle("~/test");
                bundle.AddReference("~/other");
                
                cache.SaveBundleContainer(new BundleContainer(new[] { bundle, new TestableBundle("~/other")  }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("<Reference Path=\"~/other\"");
            }
        }

        [Fact]
        public void GivenAssetWithReferenceToAnotherBundle_WhenSaveContainer_ThenXmlHasReferenceElementWithReferencedBundlePath()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle1 = new TestableBundle("~/bundle-1");
                var bundle2 = new TestableBundle("~/bundle-2");
                var asset1 = StubAsset();
                var reference = new AssetReference("~/bundle-2", asset1.Object, -1, AssetReferenceType.DifferentBundle);
                asset1.SetupGet(a => a.References)
                      .Returns(new[] { reference });
                bundle1.Assets.Add(asset1.Object);

                cache.SaveBundleContainer(new BundleContainer(new[] { bundle1, bundle2 }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("<Reference Path=\"~/bundle-2\" />");
            }
        }

        [Fact]
        public void GivenAssetWithReferenceToAssetInAnotherBundle_WhenSaveContainer_ThenXmlHasReferenceElementWithReferencedBundlePath()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle1 = new TestableBundle("~/bundle-1");
                var bundle2 = new TestableBundle("~/bundle-2");
                var asset1 = StubAsset();
                var reference = new AssetReference("~/bundle-2/asset2.js", asset1.Object, -1, AssetReferenceType.DifferentBundle);
                asset1.SetupGet(a => a.References)
                      .Returns(new[] { reference });
                bundle1.Assets.Add(asset1.Object);

                bundle2.Assets.Add(StubAsset("~/bundle-2/asset2.js").Object);

                cache.SaveBundleContainer(new BundleContainer(new[] { bundle1, bundle2 }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("<Reference Path=\"~/bundle-2\" />");
            }
        }

        [Fact]
        public void GivenAssetWithTwoReferencesToAssetsInAnotherBundle_WhenSaveContainer_ThenXmlHasOneReferenceElementWithReferencedBundlePath()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle1 = new TestableBundle("~/bundle-1");
                var bundle2 = new TestableBundle("~/bundle-2");
                var asset1 = StubAsset();
                var reference1 = new AssetReference("~/bundle-2/asset2.js", asset1.Object, -1, AssetReferenceType.DifferentBundle);
                var reference2 = new AssetReference("~/bundle-2/asset3.js", asset1.Object, -1, AssetReferenceType.DifferentBundle);
                asset1.SetupGet(a => a.References)
                      .Returns(new[] { reference1, reference2 });
                bundle1.Assets.Add(asset1.Object);

                bundle2.Assets.Add(
                    new ConcatenatedAsset(new[]
                    {
                        StubAsset("~/bundle-2/asset2.js").Object, 
                        StubAsset("~/bundle-2/asset3.js").Object
                    })
                );

                cache.SaveBundleContainer(new BundleContainer(new[] { bundle1, bundle2 }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                Regex.Matches(xml, Regex.Escape("<Reference Path=\"~/bundle-2\" />")).Count.ShouldEqual(1);
            }
        }

        [Fact]
        public void GivenAssetWithReferenceToAssetInSameBundle_WhenSaveContainer_ThenXmlHasNoReferenceElements()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle = new TestableBundle("~/bundle-1");
                var asset1 = StubAsset("~/bundle-1/asset1.js");
                var asset2 = StubAsset("~/bundle-1/asset2.js");
                var reference = new AssetReference("~/bundle-1/asset2.js", asset1.Object, -1, AssetReferenceType.SameBundle);
                asset1.SetupGet(a => a.References)
                      .Returns(new[] { reference });
                bundle.Assets.Add(new ConcatenatedAsset(new[]
                {
                    asset1.Object,
                    asset2.Object
                }));

                cache.SaveBundleContainer(new BundleContainer(new[] { bundle }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldNotContain("<Reference ");
            }
        }

        [Fact]
        public void GivenAssetWithReferenceToUrl_WhenSaveContainer_ThenXmlHasReferenceElementWithUrlAsPath()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle = new TestableBundle("~/bundle-1");
                var asset = StubAsset();
                var reference = new AssetReference("http://test.com", asset.Object, -1, AssetReferenceType.Url);
                asset.SetupGet(a => a.References)
                     .Returns(new[] { reference });
                bundle.Assets.Add(asset.Object);

                cache.SaveBundleContainer(new BundleContainer(new Bundle[] { bundle, new ExternalScriptBundle("http://test.com"),  }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("<Reference Path=\"http://test.com\" />");
            }
        }

        [Fact]
        public void GivenAssetWithRawFilenameReference_WhenSaveContainer_ThenXmlHasFileElement()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle = new TestableBundle("~/bundle-1");
                var asset = StubAsset();
                var reference = new AssetReference("~/images/test.png", asset.Object, -1, AssetReferenceType.RawFilename);
                asset.SetupGet(a => a.References)
                     .Returns(new[] { reference });
                bundle.Assets.Add(asset.Object);

                cache.SaveBundleContainer(new BundleContainer(new[] { bundle }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("<File Path=\"~/images/test.png\" />");
            }                
        }

        [Fact]
        public void GivenBundleWithAsset_WhenSaveContainer_ThenBundleFileSavedWithContentsOfAsset()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle = new TestableBundle("~/test");
                var asset = StubAsset();
                asset.Setup(a => a.OpenStream())
                     .Returns(() => "ASSET-DATA".AsStream());
                bundle.Assets.Add(asset.Object);

                cache.SaveBundleContainer(new BundleContainer(new[] { bundle }));

                var savedData = File.ReadAllText(Path.Combine(cacheDir, "test.bundle"));
                savedData.ShouldEqual("ASSET-DATA");
            }
        }

        [Fact]
        public void GivenBundleWithNoAssets_WhenSaveContainer_ThenNoBundleFileCreated()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle = new TestableBundle("~/test");

                cache.SaveBundleContainer(new BundleContainer(new[] { bundle }));

                File.Exists(Path.Combine(cacheDir, "test.bundle")).ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenBundleWithTwoAssets_WhenSaveContainer_ThenExceptionThrownBecauseAssetsMustBeConcatenatedBeforeCaching()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle = new TestableBundle("~/test");
                bundle.Assets.Add(StubAsset().Object);
                bundle.Assets.Add(StubAsset().Object);

                Assert.Throws<InvalidOperationException>(
                    () => cache.SaveBundleContainer(new BundleContainer(new[] { bundle }))
                );
            }
        }

        [Fact]
        public void GivenBundlePathIsUrl_WhenSaveContainer_ThenThrowException()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var bundle = new TestableBundle("http://test.com/api.js");
                bundle.Assets.Add(StubAsset().Object);

                Assert.Throws<ArgumentException>(
                    () => cache.SaveBundleContainer(new BundleContainer(new[] { bundle }))
                );
            }
        }

        [Fact]
        public void GivenCachedFileThrowsWhenDeleted_WhenSaveContainer_ThenSecondCachedFileIsStillDeleted()
        {
            var firstFile = new Mock<IFile>();
            firstFile.Setup(f => f.Delete()).Throws<IOException>();
            var secondFile = new Mock<IFile>();
            var cacheDirectory = new Mock<IDirectory>();
            cacheDirectory.Setup(d => d.GetFiles("*", SearchOption.AllDirectories))
                          .Returns(new[] { firstFile.Object, secondFile.Object });
            var containerFile = new Mock<IFile>();
            cacheDirectory.Setup(d => d.GetFile("container.xml"))
                          .Returns(containerFile.Object);
            containerFile.Setup(f => f.Open(It.IsAny<FileMode>(), It.IsAny<FileAccess>(), It.IsAny<FileShare>()))
                         .Returns(Stream.Null);

            var settings = new CassetteSettings("")
            {
                SourceDirectory = Mock.Of<IDirectory>(),
                CacheDirectory = cacheDirectory.Object
            };

            var cache = new BundleCache("VERSION", settings);
            cache.SaveBundleContainer(new BundleContainer(Enumerable.Empty<Bundle>()));

            secondFile.Verify(f => f.Delete());
        }

        Mock<IAsset> StubAsset(string path = null)
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns(path);
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            asset.Setup(a => a.OpenStream()).Returns(Stream.Null);
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            return asset;
        }
    }

    public class BundleCache_InitializeBundlesFromCacheIfUpToDate_Tests
    {
        [Fact]
        public void GivenContainerFileDoesNotExist_WhenInitializeBundlesFromCacheIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            {
                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var sourceBundles = new TestableBundle[0];

                var result = cache.InitializeBundlesFromCacheIfUpToDate(sourceBundles);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenContainerFileWithDifferentVersion_WhenInitializeBundlesFromCacheIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION-1\" AssetCount=\"0\"></Container>"
                    );

                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION-2", settings);
                var sourceBundles = new TestableBundle[0];

                var result = cache.InitializeBundlesFromCacheIfUpToDate(sourceBundles);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenContainerFileWithDifferentAssetCount_WhenInitializeBundlesFromCacheIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\"></Container>"
                    );

                var bundleWithAsset = new TestableBundle("~/test");
                var asset = StubAsset();
                bundleWithAsset.Assets.Add(asset.Object);
                var sourceBundles = new[] { bundleWithAsset };

                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var result = cache.InitializeBundlesFromCacheIfUpToDate(sourceBundles);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenContainerFileIsOlderThanAnAssetFile_WhenInitializeBundlesFromCacheIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\"></Container>"
                    );

                var bundleWithAsset = new TestableBundle("~/test");
                var asset = StubAsset();
                asset.Setup(a => a.SourceFile.LastWriteTimeUtc).Returns(DateTime.UtcNow);
                bundleWithAsset.Assets.Add(asset.Object);
                var sourceBundles = new[] { bundleWithAsset };

                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var result = cache.InitializeBundlesFromCacheIfUpToDate(sourceBundles);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenCacheIsUpToDate_WhenInitializeBundlesFromCacheIfUpToDate_ThenReturnTrue()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\"><Bundle Path=\"~/test\" Hash=\"01\"/></Container>"
                    );
                File.WriteAllText(
                    Path.Combine(cacheDir, "test.bundle"),
                    "asset"
                    );

                var bundle = new TestableBundle("~/test");
                var sourceBundles = new[] { bundle };

                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var result = cache.InitializeBundlesFromCacheIfUpToDate(sourceBundles);

                result.ShouldBeTrue();
            }
        }

        [Fact]
        public void GivenCacheIsUpToDate_WhenInitializeBundlesFromCacheIfUpToDate_ThenBundleAssetsReplacedWithCachedAsset()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"1\"><Bundle Path=\"~/test\" Hash=\"01\"/></Container>"
                    );
                File.WriteAllText(
                    Path.Combine(cacheDir, "test.bundle"),
                    "asset"
                    );
                var bundleWithAsset = new TestableBundle("~/test");
                var asset = StubAsset();
                bundleWithAsset.Assets.Add(asset.Object);
                var sourceBundles = new[] { bundleWithAsset };

                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var result = cache.InitializeBundlesFromCacheIfUpToDate(sourceBundles);

                result.ShouldBeTrue();
                bundleWithAsset.Assets[0].OpenStream().ReadToEnd().ShouldEqual("asset");
            }
        }

        [Fact]
        public void GivenCacheIsUpToDate_WhenInitializeBundlesFromCacheIfUpToDateWithZeroAssetBundle_ThenReturnTrue()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\"><Bundle Path=\"~/test\" Hash=\"\"/></Container>"
                    );
                var bundle = new TestableBundle("~/test");
                var sourceBundles = new[] { bundle };

                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var result = cache.InitializeBundlesFromCacheIfUpToDate(sourceBundles);

                result.ShouldBeTrue();
            }
        }

        [Fact]
        public void GivenCacheMissingSecondFile_WhenInitializeBundlesFromCacheIfUpToDate_ThenFirstBundleAssetsAreNotModified()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"2\"><Bundle Path=\"~/test1\" Hash=\"01\"/><Bundle Path=\"~/test2\" Hash=\"01\"/></Container>"
                    );
                File.WriteAllText(
                    Path.Combine(cacheDir, "test1.bundle"),
                    "asset"
                    );
                var bundle1 = new TestableBundle("~/test1");
                var bundle2 = new TestableBundle("~/test2");
                var asset1 = StubAsset();
                bundle1.Assets.Add(asset1.Object);
                var asset2 = StubAsset();
                bundle2.Assets.Add(asset2.Object);
                var sourceBundles = new[] { bundle1, bundle2 };

                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var result = cache.InitializeBundlesFromCacheIfUpToDate(sourceBundles);

                result.ShouldBeFalse();
                bundle1.Assets[0].ShouldNotBeType<CachedAsset>();
            }
        }

        [Fact]
        public void GivenContainerCacheHasBundleReferences_WhenInitializeBundlesFromCacheIfUpToDate_ThenBundleReferencesAreSet()
        {
            using (var cacheDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\">" +
                    "<Bundle Path=\"~/test1\" Hash=\"01\">" +
                    "  <Reference Path=\"~/test2\"/>" +
                    "</Bundle>" +
                    "<Bundle Path=\"~/test2\" Hash=\"01\"/>" +
                    "</Container>"
                    );
                var bundle1 = new TestableBundle("~/test1");
                var bundle2 = new TestableBundle("~/test2");
                var sourceBundles = new[] { bundle1, bundle2 };

                var settings = new CassetteSettings("")
                {
                    SourceDirectory = Mock.Of<IDirectory>(),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                cache.InitializeBundlesFromCacheIfUpToDate(sourceBundles);

                bundle1.References.First().ShouldEqual("~/test2");
            }
        }

        [Fact]
        public void GivenContainerCacheHasFileReferenceThatIsModifiedSinceCacheWrite_WhenInitializeBundlesFromCacheIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            using (var sourceDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\">" +
                    "<Bundle Path=\"~/test\" Hash=\"01\">" +
                    "  <File Path=\"~/file.png\"/>" +
                    "</Bundle>" +
                    "</Container>"
                );
                Thread.Sleep(10); // Brief pause, otherwise the file write times are the same.
                File.WriteAllText(Path.Combine(sourceDir, "file.png"), "");
                var bundle = new TestableBundle("~/test");
                var sourceBundles = new[] { bundle };

                var settings = new CassetteSettings("")
                {
                    SourceDirectory = new FileSystemDirectory(sourceDir),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var result = cache.InitializeBundlesFromCacheIfUpToDate(sourceBundles);

                result.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenContainerCacheHasFileReferenceToMissingFile_WhenInitializeBundlesFromCacheIfUpToDate_ThenReturnFalse()
        {
            using (var cacheDir = new TempDirectory())
            using (var sourceDir = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(cacheDir, "container.xml"),
                    "<?xml version=\"1.0\"?><Container Version=\"VERSION\" AssetCount=\"0\">" +
                    "<Bundle Path=\"~/test\" Hash=\"01\">" +
                    "  <File Path=\"~/file.png\"/>" +
                    "</Bundle>" +
                    "</Container>"
                );
                var bundle = new TestableBundle("~/test");
                var sourceBundles = new[] { bundle };


                var settings = new CassetteSettings("")
                {
                    SourceDirectory = new FileSystemDirectory(sourceDir),
                    CacheDirectory = new FileSystemDirectory(cacheDir)
                };
                var cache = new BundleCache("VERSION", settings);
                var result = cache.InitializeBundlesFromCacheIfUpToDate(sourceBundles);

                result.ShouldBeFalse();
            }
        }

        Mock<IAsset> StubAsset()
        {
            var asset = new Mock<IAsset>();

            asset.Setup(a => a.SourceFile.LastWriteTimeUtc)
                 .Returns(new DateTime(2000,1,1));

            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));

            return asset;
        }
    }
}

