using System;
using Should;
using Xunit;

namespace Cassette
{
    public class AssetReference_Tests
    {
        [Fact]
        public void ConstructorAssignsProperties()
        {
            var asset = new StubAsset();
            var reference = new AssetReference(asset.Path, "~/path", 1, AssetReferenceType.DifferentBundle);

            reference.FromAssetPath.ShouldBeSameAs(asset.Path);
            reference.ToPath.ShouldEqual("~/path");
            reference.SourceLineNumber.ShouldEqual(1);
            reference.Type.ShouldEqual(AssetReferenceType.DifferentBundle);
        }

        [Fact]
        public void WhenCreateWithDifferentBundleTypeAndPathNotStartingWithTilde_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new AssetReference(null, "fail", -1, AssetReferenceType.DifferentBundle)
            );
        }

        [Fact]
        public void WhenCreateWithSameBundleTypeAndPathNotStartingWithTilde_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new AssetReference(null, "fail", -1, AssetReferenceType.SameBundle)
            );
        }

        [Fact]
        public void WhenCreateWithRawFilenameTypeAndPathNotStartingWithTilde_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new AssetReference(null, "fail", -1, AssetReferenceType.RawFilename)
            );
        }

        [Fact]
        public void WhenCreateWithUrlTypeAndPathIsNotUrl_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new AssetReference(null, "not-a-url", -1, AssetReferenceType.Url)
            );
        }
    }
}

