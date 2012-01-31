using System.IO;
using System.Linq;
using System.Text;
using Cassette.Configuration;
using Cassette.Manifests;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts.Manifests
{
    public class ScriptBundleManifestBuilder_Tests
    {
        readonly ScriptBundleManifestBuilder builder = new ScriptBundleManifestBuilder { IncludeContent = true };
        readonly ScriptBundle bundle;
        readonly IAsset asset;
        BundleManifest manifest;
        readonly byte[] bundleContent = Encoding.UTF8.GetBytes("bundle-content");

        public ScriptBundleManifestBuilder_Tests()
        {
            bundle = new ScriptBundle("~/path") { PageLocation = "body", Hash = new byte[] { 1, 2, 3 } };
            asset = StubAsset();
            bundle.Assets.Add(asset);
            bundle.AddReference("~/reference/path");
            bundle.Process(new CassetteSettings(""));

            manifest = builder.BuildManifest(bundle);
        }

        [Fact]
        public void ManifestPathEqualsBundlePath()
        {
            manifest.Path.ShouldEqual(bundle.Path);
        }

        [Fact]
        public void ManifestHashEqualsBundleHash()
        {
            manifest.Hash.ShouldEqual(bundle.Hash);
        }

        [Fact]
        public void ManifestContentTypeEqualsBundleContentType()
        {
            manifest.ContentType.ShouldEqual(bundle.ContentType);
        }

        [Fact]
        public void ManifestPageLocationEqualsBundlePageLocation()
        {
            manifest.PageLocation.ShouldEqual(bundle.PageLocation);
        }

        [Fact]
        public void ManifestAssetsContainsSingleAsset()
        {
            manifest.Assets.Count().ShouldEqual(1);
        }

        [Fact]
        public void AssetManifestPathEqualsAssetSourceFileFullPath()
        {
            manifest.Assets[0].Path.ShouldEqual(asset.SourceFile.FullPath);
        }

        [Fact]
        public void ManifestReferencesEqualsBundleReferences()
        {
            manifest.References.SequenceEqual(bundle.References).ShouldBeTrue();
        }

        [Fact]
        public void AssetManifestRawFileReferencesEqualsAssetRawFileReferences()
        {
            var rawFileReference = manifest.Assets[0].References[0];
            rawFileReference.Path.ShouldEqual(asset.References.First().Path);
        }

        [Fact]
        public void ManifestContentEqualsBytesFromBundleOpenStream()
        {
            manifest.Content.ShouldEqual(bundleContent);
        }

        [Fact]
        public void GivenBundleWithNoAssetsThenManifestContentIsNull()
        {
            bundle.Assets.Clear();
            manifest = builder.BuildManifest(bundle);
            manifest.Content.ShouldBeNull();
        }

        [Fact]
        public void GivenIncludeContentIsFalse_ThenManifestContentIsNull()
        {
            builder.IncludeContent = false;
            manifest = builder.BuildManifest(bundle);
            manifest.Content.ShouldBeNull();
        }

        IAsset StubAsset()
        {
            var stubAsset = new Mock<IAsset>();
            stubAsset.SetupGet(a => a.SourceFile.FullPath).Returns("~/path/asset");
            stubAsset.SetupGet(a => a.References).Returns(new[]
            {
                new AssetReference("~/path/asset/file", stubAsset.Object, 0, AssetReferenceType.RawFilename)
            });
            stubAsset.Setup(a => a.OpenStream()).Returns(() => new MemoryStream(bundleContent));

            stubAsset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                     .Callback<IBundleVisitor>(v => v.Visit(stubAsset.Object));

            return stubAsset.Object;
        }
    }
}