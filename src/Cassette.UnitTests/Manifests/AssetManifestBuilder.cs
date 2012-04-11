using System.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class AssetManifestBuilder_Tests
    {
        readonly AssetManifest manifest;
        readonly AssetReference bundleReference;
        readonly AssetReference urlReference;
        readonly AssetReference rawFileReference;

        public AssetManifestBuilder_Tests()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/asset");
            bundleReference = new AssetReference("~/bundle", asset.Object, 1, AssetReferenceType.DifferentBundle);
            urlReference = new AssetReference("http://example.com/", asset.Object, 2, AssetReferenceType.Url);
            rawFileReference = new AssetReference("~/file", asset.Object, 3, AssetReferenceType.RawFilename);
            var sameBundleReference = new AssetReference("~/same", asset.Object, 4, AssetReferenceType.SameBundle);
            asset.SetupGet(a => a.References)
                 .Returns(new[] { bundleReference, urlReference, rawFileReference, sameBundleReference });

            var builder = new AssetManifestBuilder();
            manifest = builder.BuildManifest(asset.Object);
        }

        [Fact]
        public void ManifestPathEqualsAssetPath()
        {
            manifest.Path.ShouldEqual("~/asset");
        }

        [Fact]
        public void FirstAssetManifestReferenceEqualsBundleReference()
        {
            ManifestReferenceEqualsAssetReference(manifest.References[0], bundleReference);
        }

        [Fact]
        public void SecondAssetManifestReferenceEqualsUrlReference()
        {
            ManifestReferenceEqualsAssetReference(manifest.References[1], urlReference);
        }

        [Fact]
        public void ManifestDoesNotContainSameBundleReferences()
        {
            manifest.References.Where(r => r.Type == AssetReferenceType.SameBundle).ShouldBeEmpty();
        }

        void ManifestReferenceEqualsAssetReference(AssetReferenceManifest referenceManifest, AssetReference reference)
        {
            referenceManifest.Path.ShouldEqual(reference.Path);
            referenceManifest.Type.ShouldEqual(reference.Type);
            referenceManifest.SourceLineNumber.ShouldEqual(reference.SourceLineNumber);
        }
    }
}