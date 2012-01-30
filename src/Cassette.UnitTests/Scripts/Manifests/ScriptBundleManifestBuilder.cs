using System.Linq;
using Cassette.Manifests;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts.Manifests
{
    public class ScriptBundleManifestBuilder_Tests
    {
        readonly ScriptBundleManifestBuilder builder = new ScriptBundleManifestBuilder();
        readonly ScriptBundle bundle;
        readonly IAsset asset;
        readonly BundleManifest manifest;

        public ScriptBundleManifestBuilder_Tests()
        {
            bundle = new ScriptBundle("~/path") { PageLocation = "body" };
            asset = StubAsset();
            bundle.Assets.Add(asset);
            bundle.AddReference("~/reference/path");

            manifest = builder.BuildManifest(bundle);
        }

        [Fact]
        public void ManifestPathEqualsBundlePath()
        {
            manifest.Path.ShouldEqual(bundle.Path);
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
            var rawFileReference = manifest.Assets[0].RawFileReferences[0];
            rawFileReference.ShouldEqual(asset.References.First().Path);
        }

        IAsset StubAsset()
        {
            var stubAsset = new Mock<IAsset>();
            stubAsset.SetupGet(a => a.SourceFile.FullPath).Returns("~/path/asset");
            stubAsset.SetupGet(a => a.References).Returns(new[]
            {
                new AssetReference("~/path/asset/file", stubAsset.Object, 0, AssetReferenceType.RawFilename)
            });

            stubAsset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                     .Callback<IBundleVisitor>(v => v.Visit(stubAsset.Object));

            return stubAsset.Object;
        }
    }
}