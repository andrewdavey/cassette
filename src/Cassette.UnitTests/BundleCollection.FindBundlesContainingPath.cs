using System.Linq;
using Cassette.Configuration;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleCollection_FindBundlesContainingPath
    {
        [Fact]
        public void FindBundleContainingPathOfBundleReturnsTheBundle()
        {
            var expectedBundle = new TestableBundle("~/test");
            var collection = new BundleCollection(new CassetteSettings(""), t => null, Mock.Of<IBundleFactoryProvider>())
            {
                expectedBundle
            };
            var actualBundle = collection.FindBundlesContainingPath("~/test").First();
            actualBundle.ShouldBeSameAs(expectedBundle);
        }

        [Fact]
        public void FindBundleContainingPathOfBundleWherePathIsMissingRootPrefixReturnsTheBundle()
        {
            var expectedBundle = new TestableBundle("~/test");
            var collection = new BundleCollection(new CassetteSettings(""), t => null, Mock.Of<IBundleFactoryProvider>())
            {
                expectedBundle
            };
            var actualBundle = collection.FindBundlesContainingPath("test").First();
            actualBundle.ShouldBeSameAs(expectedBundle);
        }

        [Fact]
        public void FindBundleContainingPathWithWrongPathReturnsNull()
        {
            var collection = new BundleCollection(new CassetteSettings(""), t => null, Mock.Of<IBundleFactoryProvider>())
            {
                new TestableBundle("~/test")
            };
            var actualBundle = collection.FindBundlesContainingPath("~/WRONG");
            actualBundle.ShouldBeEmpty();
        }

        [Fact]
        public void FindBundleContainingPathOfAssetReturnsTheBundle()
        {
            var expectedBundle = new TestableBundle("~/test");
            var asset = new Mock<IAsset>();
            AssetAcceptsVisitor(asset);
            asset.SetupGet(a => a.Path).Returns("~/test/test.js");
            expectedBundle.Assets.Add(asset.Object);
            var container = new BundleContainer(new[] {
                expectedBundle
            });
            var actualBundle = container.FindBundlesContainingPath("~/test/test.js").First();
            actualBundle.ShouldBeSameAs(expectedBundle);
        }

        void AssetAcceptsVisitor(Mock<IAsset> asset)
        {
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
        }
    }
}