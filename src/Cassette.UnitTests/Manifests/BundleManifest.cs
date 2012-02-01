using System;
using System.IO;
using Cassette.IO;
using Cassette.Scripts.Manifests;
using Cassette.Stylesheets.Manifests;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class BundleManifest_Equals_Tests
    {
        class TestableBundleManifest : BundleManifest
        {
            protected override Bundle CreateBundleCore()
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void BundleManifestsWithSamePathAndNoAssetsAreEqual()
        {
            var manifest1 = new TestableBundleManifest { Path = "~/path" };
            var manifest2 = new TestableBundleManifest { Path = "~/path" };
            manifest1.Equals(manifest2).ShouldBeTrue();
        }

        [Fact]
        public void BundleManifestsWithSamePathsButDiffentCasingAreEqual()
        {
            var manifest1 = new TestableBundleManifest { Path = "~/PATH" };
            var manifest2 = new TestableBundleManifest { Path = "~/path" };
            manifest1.Equals(manifest2).ShouldBeTrue();
        }

        [Fact]
        public void BundleManifestsOfDifferentTypeAreNotEqual()
        {
            BundleManifest manifest1 = new StylesheetBundleManifest { Path = "~/path" };
            BundleManifest manifest2 = new ScriptBundleManifest { Path = "~/path" };
            manifest1.Equals(manifest2).ShouldBeFalse();
        }

        [Fact]
        public void BundleManifestsWithDifferentAssetsAreNotEqual()
        {
            var manifest1 = new TestableBundleManifest
            {
                Path = "~/path",
                Assets = { new AssetManifest { Path = "~/asset-path" } }
            };
            var manifest2 = new TestableBundleManifest
            {
                Path = "~/path",
                Assets = { new AssetManifest { Path = "~/different-asset-path" } }
            };
            manifest1.Equals(manifest2).ShouldBeFalse();
        }

        [Fact]
        public void BundleManifestsWithSameAssetsAreEqual()
        {
            var manifest1 = new TestableBundleManifest
            {
                Path = "~/path",
                Assets = { new AssetManifest { Path = "~/asset-path" } }
            };
            var manifest2 = new TestableBundleManifest
            {
                Path = "~/path",
                Assets = { new AssetManifest { Path = "~/asset-path" } }
            };
            manifest1.Equals(manifest2).ShouldBeTrue();
        }

        [Fact]
        public void BundleManifestsAreEqualEvenIfAssetsAreInDifferentOrders()
        {
            var manifest1 = new TestableBundleManifest
            {
                Path = "~/path",
                Assets =
                    {
                        new AssetManifest { Path = "~/A" },
                        new AssetManifest { Path = "~/B" }
                    }
            };
            var manifest2 = new TestableBundleManifest
            {
                Path = "~/path",
                Assets =
                    {
                        new AssetManifest { Path = "~/B" },
                        new AssetManifest { Path = "~/A" }
                    }
            };
            manifest1.Equals(manifest2).ShouldBeTrue();
        }
    }

    public class BundleManifest_CreateBundle_Tests
    {
        readonly TestableBundleManifest manifest;

        public BundleManifest_CreateBundle_Tests()
        {
            manifest = new TestableBundleManifest
            {
                Path = "~",
                Hash = new byte[0]
            };
        }

        [Fact]
        public void GivenManifestHasContent_WhenCreateBundle_ThenBundleOpenStreamReturnsTheContent()
        {
            manifest.Assets.Add(new AssetManifest { Path = "~/asset" });
            manifest.Content = new byte[] { 1, 2, 3 };
            var bundle = manifest.CreateBundle();

            using (var stream = bundle.OpenStream())
            {
                var bytes = new byte[3];
                stream.Read(bytes, 0, 3);
                bytes.ShouldEqual(new byte[] { 1, 2, 3 });
            }
        }

        [Fact]
        public void GivenManifestHasNoContent_WhenCreateBundle_ThenBundleOpenStreamReturnsEmpty()
        {
            manifest.Content = null;
            var bundle = manifest.CreateBundle();

            bundle.OpenStream().Length.ShouldEqual(0);
        }

        [Fact]
        public void GivenManifestWithReferences_WhenCreateBundle_ThenBundleHasTheReferences()
        {
            manifest.References.Add("~/reference");
            var bundle = manifest.CreateBundle();
            bundle.References.ShouldEqual(new[] { "~/reference" });
        }

        [Fact]
        public void GivenManifestWithHtmlAttributes_WhenCreateBundle_WhenBundleHasHtmlAttributes()
        {
            manifest.HtmlAttributes.Add("attribute", "value");

            var bundle = manifest.CreateBundle();

            bundle.HtmlAttributes["attribute"].ShouldEqual("value");
        }

        [Fact]
        public void GivenManifestWithHtmlAttributeWhereValueIsNull_WhenCreateBundle_WhenBundleHasHtmlAttribute()
        {
            manifest.HtmlAttributes.Add("attribute", null);

            var bundle = manifest.CreateBundle();

            bundle.HtmlAttributes["attribute"].ShouldBeNull();
        }

        class TestableBundleManifest : BundleManifest
        {
            protected override Bundle CreateBundleCore()
            {
                return new TestableBundle(Path);
            }
        }
    }

    public class BundleManifest_IsUpToDateWithFileSystem_Tests : IDisposable
    {
        readonly IDirectory directory;
        readonly TestableBundleManifest manifest;
        readonly TempDirectory tempDirectory;
        readonly DateTime yesterday = DateTime.UtcNow.AddDays(-1);
        DateTime cacheWriteTime;

        public BundleManifest_IsUpToDateWithFileSystem_Tests()
        {
            tempDirectory = new TempDirectory();
            manifest = new TestableBundleManifest();
            directory = new FileSystemDirectory(tempDirectory);
        }

        [Fact]
        public void EmptyManifestIsUpToDateWithFileSystem()
        {
            ManifestIsUpToDateWithFileSystem();
        }

        [Fact]
        public void GivenAssetFileExistsAndManifestWrittenLater_ThenManifestIsUpToDateWithFileSystem()
        {
            CreateFile("asset.js");

            manifest.Assets.Add(new AssetManifest { Path = "~/asset.js" });
            cacheWriteTime = DateTime.UtcNow;

            ManifestIsUpToDateWithFileSystem();
        }

        [Fact]
        public void GivenAssetFileWrittenAfterManifest_ThenManifestIsNotUpToDateWithFileSystem()
        {
            manifest.Assets.Add(new AssetManifest { Path = "~/asset.js" });
            CacheWasCreatedYesterday();

            CreateFile("asset.js");

            ManifestIsNotUpToDateWithFileSystem();
        }

        [Fact]
        public void GivenAssetFileDoesNotExist_ThenManifestIsNotUpToDateWithFileSystem()
        {
            manifest.Assets.Add(new AssetManifest { Path = "~/asset.js" });
            ManifestIsNotUpToDateWithFileSystem();
        }

        [Fact]
        public void GivenAssetRawFileReferenceToFileNewerThanManifest_ThenManifestIsNotUpToDateWithFileSystem()
        {
            FileWasCreatedYesterday("asset.css");
            
            manifest.Assets.Add(new AssetManifest
            {
                Path = "~/asset.css",
                References = { ImageReference() }
            });
            CacheWasCreatedYesterday();

            CreateFile("image.png");

            ManifestIsNotUpToDateWithFileSystem();
        }

        [Fact]
        public void GivenAssetRawFileReferenceToFileThatDoesNotExist_ThenManifestIsNotUpToDateWithFileSystem()
        {
            CreateFile("asset.css");

            manifest.Assets.Add(new AssetManifest
            {
                Path = "~/asset.css",
                References = { ImageReference() }
            });

            ManifestIsNotUpToDateWithFileSystem();
        }

        void CreateFile(string path)
        {
            File.WriteAllText(Path.Combine(tempDirectory, path), "");
        }

        void FileWasCreatedYesterday(string path)
        {
            CreateFile(path);
            File.SetLastWriteTimeUtc(Path.Combine(tempDirectory, path), yesterday);
        }

        void CacheWasCreatedYesterday()
        {
            cacheWriteTime = yesterday;
        }

        AssetReferenceManifest ImageReference()
        {
            return new AssetReferenceManifest
            {
                Path = "~/image.png",
                Type = AssetReferenceType.RawFilename
            };
        }

        void ManifestIsUpToDateWithFileSystem()
        {
            manifest.IsUpToDateWithFileSystem(directory, cacheWriteTime).ShouldBeTrue();
        }

        void ManifestIsNotUpToDateWithFileSystem()
        {
            manifest.IsUpToDateWithFileSystem(directory, cacheWriteTime).ShouldBeFalse();
        }

        void IDisposable.Dispose()
        {
            tempDirectory.Dispose();
        }

        class TestableBundleManifest : BundleManifest
        {
            protected override Bundle CreateBundleCore()
            {
                throw new NotImplementedException();
            }
        }
    }
}