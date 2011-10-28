#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Cassette.IO;
using Cassette.BundleProcessing;
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
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var bundle = new TestableBundle("~/test");
                var asset1 = StubAsset();
                bundle.Assets.Add(asset1.Object);

                cache.SaveBundleContainer(new BundleContainer(new[] { bundle }));

                var xml = File.ReadAllText(Path.Combine(cacheDir, "container.xml"));
                xml.ShouldContain("<Bundle Path=\"~/test\"");
            }
        }

        [Fact]
        public void GivenBundleWithReference_WhenSaveContainer_ThenXmlHasReferenceElement()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var bundle = new TestableBundle("~/test");
                bundle.Assets.Add(StubAsset().Object);
                bundle.Assets.Add(StubAsset().Object);

                Assert.Throws<InvalidOperationException>(
                    () => cache.SaveBundleContainer(new BundleContainer(new[] { bundle }))
                );
            }
        }

        [Fact]
        public void GivenBundlePathIsUrl_WhenSaveContainer_ThenBundleFileNameIsEncoded()
        {
            using (var cacheDir = new TempDirectory())
            {
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
                var bundle = new TestableBundle("http://test.com/api.js");
                bundle.Assets.Add(StubAsset().Object);

                cache.SaveBundleContainer(new BundleContainer(new[] { bundle }));

                File.Exists(Path.Combine(cacheDir, "http%3A%2F%2Ftest.com%2Fapi.js.bundle")).ShouldBeTrue();
            }
        }

        Mock<IAsset> StubAsset(string path = null)
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns(path);
            asset.SetupGet(a => a.Hash).Returns(new byte[0]);
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
                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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

                var cache = new BundleCache("VERSION-2", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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

                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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

                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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

                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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

                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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

                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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

                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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

                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), Mock.Of<IDirectory>());
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

                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), new FileSystemDirectory(sourceDir));
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

                var cache = new BundleCache("VERSION", new FileSystemDirectory(cacheDir), new FileSystemDirectory(sourceDir));
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

