using System;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class AssetReference_Tests
    {
        [Fact]
        public void ConstructorAssignsProperties()
        {
            var asset = Mock.Of<IAsset>();
            var reference = new AssetReference("~/path", asset, 1, AssetReferenceType.DifferentBundle);

            reference.Path.ShouldEqual("~/path");
            reference.SourceAsset.ShouldBeSameAs(asset);
            reference.SourceLineNumber.ShouldEqual(1);
            reference.Type.ShouldEqual(AssetReferenceType.DifferentBundle);
        }

        [Fact]
        public void WhenCreateWithDifferentBundleTypeAndPathNotStartingWithTilde_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new AssetReference("fail", null, -1, AssetReferenceType.DifferentBundle)
            );
        }

        [Fact]
        public void WhenCreateWithSameBundleTypeAndPathNotStartingWithTilde_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new AssetReference("fail", null, -1, AssetReferenceType.SameBundle)
            );
        }

        [Fact]
        public void WhenCreateWithRawFilenameTypeAndPathNotStartingWithTilde_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new AssetReference("fail", null, -1, AssetReferenceType.RawFilename)
            );
        }

        [Fact]
        public void WhenCreateWithUrlTypeAndPathIsNotUrl_ThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(
                () => new AssetReference("not-a-url", null, -1, AssetReferenceType.Url)
            );
        }
    }
}
