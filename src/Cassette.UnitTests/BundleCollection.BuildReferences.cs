using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleCollection_BuildReferences
    {
        readonly BundleCollection collection;

        public BundleCollection_BuildReferences()
        {
            collection = new BundleCollection(new CassetteSettings(), Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
        }

        [Fact]
        public void GivenAssetWithUnknownDifferentBundleReference_ThenBuildReferencesThrowsAssetReferenceException()
        {
            var bundle = new TestableBundle("~/bundle-1");
            var asset = new Mock<IAsset>();
            AssetAcceptsVisitor(asset);
            asset.SetupGet(a => a.Path).Returns("bundle-1\\a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference(asset.Object.Path, "~\\fail\\fail.js", 0, AssetReferenceType.DifferentBundle) });
            bundle.Assets.Add(asset.Object);
            collection.Add(bundle);
            
            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                collection.BuildReferences();
            });

            exception.Message.ShouldEqual("Reference error in \"bundle-1\\a.js\". Cannot find \"~\\fail\\fail.js\".");
        }

        [Fact]
        public void GivenAssetWithUnknownDifferentBundleReferenceHavingLineNumber_ThenBuildReferencesThrowsAssetReferenceException()
        {
            var bundle = new TestableBundle("~/bundle-1");
            var asset = new Mock<IAsset>();
            AssetAcceptsVisitor(asset);
            asset.SetupGet(a => a.Path).Returns("~/bundle-1/a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference(asset.Object.Path, "~\\fail\\fail.js", 42, AssetReferenceType.DifferentBundle) });
            bundle.Assets.Add(asset.Object);
            collection.Add(bundle);

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                collection.BuildReferences();
            });
            exception.Message.ShouldEqual("Reference error in \"~/bundle-1/a.js\", line 42. Cannot find \"~\\fail\\fail.js\".");
        }

        [Fact]
        public void GivenBundleWithInvalid_BuildReferencesThrowsException()
        {
            var bundle1 = new TestableBundle("~/bundle1");
            bundle1.AddReference("~\\bundle2");
            collection.Add(bundle1);

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                collection.BuildReferences();
            });
            exception.Message.ShouldEqual("Reference error in bundle descriptor for \"~/bundle1\". Cannot find \"~/bundle2\".");
        }

        [Fact]
        public void GivenAssetWithReferenceToNonexistantFileAndBundleIsFromDescriptorFile_WhenBuildReferences_ThenAssetReferenceIsIgnored()
        {
            var bundle = new TestableBundle("~");
            var asset = new StubAsset();
            var badReference = new AssetReference(asset.Path, "~/NOT-FOUND.js", 1, AssetReferenceType.DifferentBundle);
            asset.References.Add(badReference);
            bundle.Assets.Add(asset);
            bundle.IsFromDescriptorFile = true;
            collection.Add(bundle);

            Assert.DoesNotThrow(
                () => collection.BuildReferences()
            );
        }

        void AssetAcceptsVisitor(Mock<IAsset> asset)
        {
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
        }
    }
}