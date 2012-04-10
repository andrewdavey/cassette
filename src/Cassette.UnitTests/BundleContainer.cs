using Cassette.Configuration;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleContainer_Tests
    {
        [Fact]
        public void GivenAssetWithUnknownDifferentBundleReference_ThenConstructorThrowsAssetReferenceException()
        {
            var bundle = new TestableBundle("~/bundle-1");
            var asset = new Mock<IAsset>();
            AssetAcceptsVisitor(asset);
            asset.SetupGet(a => a.Path).Returns("bundle-1\\a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~\\fail\\fail.js", asset.Object, 0, AssetReferenceType.DifferentBundle) });
            bundle.Assets.Add(asset.Object);

            var exception = Assert.Throws<AssetReferenceException>(
                () => BuildReferences(bundle)
            );
            exception.Message.ShouldEqual("Reference error in \"bundle-1\\a.js\". Cannot find \"~\\fail\\fail.js\".");
        }

        [Fact]
        public void GivenAssetWithUnknownDifferentBundleReferenceHavingLineNumber_ThenConstructorThrowsAssetReferenceException()
        {
            var bundle = new TestableBundle("~/bundle-1");
            var asset = new Mock<IAsset>();
            AssetAcceptsVisitor(asset);
            asset.SetupGet(a => a.Path).Returns("~/bundle-1/a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~\\fail\\fail.js", asset.Object, 42, AssetReferenceType.DifferentBundle) });
            bundle.Assets.Add(asset.Object);

            var exception = Assert.Throws<AssetReferenceException>(
                () => BuildReferences(bundle)
            );
            exception.Message.ShouldEqual("Reference error in \"~/bundle-1/a.js\", line 42. Cannot find \"~\\fail\\fail.js\".");
        }

        [Fact]
        public void GivenBundleWithInvalid_ConstructorThrowsException()
        {
            var bundle1 = new TestableBundle("~/bundle1");
            bundle1.AddReference("~\\bundle2");

            var exception = Assert.Throws<AssetReferenceException>(
                () => BuildReferences(bundle1)
            );
            exception.Message.ShouldEqual("Reference error in bundle descriptor for \"~/bundle1\". Cannot find \"~/bundle2\".");
        }

        [Fact]
        public void GivenAssetWithReferenceToNonexistantFileAndBundleIsFromDescriptorFile_WhenConstructed_ThenAssetReferenceIsIgnored()
        {
            var bundle = new TestableBundle("~");
            var asset = new StubAsset();
            var badReference = new AssetReference("~/NOT-FOUND.js", asset, 1, AssetReferenceType.DifferentBundle);
            asset.References.Add(badReference);
            bundle.Assets.Add(asset);

            bundle.IsFromDescriptorFile = true;
            
            Assert.DoesNotThrow(
                () => BuildReferences(bundle)
            );
        }

        void AssetAcceptsVisitor(Mock<IAsset> asset)
        {
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
        }

        void BuildReferences(TestableBundle bundle)
        {
            var bundles = new BundleCollection(new CassetteSettings(""), t => null, Mock.Of<IBundleFactoryProvider>());
            bundles.Add(bundle);
            bundles.BuildReferences();
        }
    }
}