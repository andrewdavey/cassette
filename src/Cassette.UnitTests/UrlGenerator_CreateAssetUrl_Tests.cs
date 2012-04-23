using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class UrlGenerator_CreateAssetUrl_Tests : UrlGeneratorTestsBase
    {
        [Fact]
        public void UrlModifierModifyIsCalled()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/test/asset.coffee");
            asset.SetupGet(a => a.Hash).Returns(new byte[0]);

            UrlGenerator.CreateAssetUrl(asset.Object);

            UrlModifier.Verify(m => m.Modify(It.IsAny<string>()));
        }

        [Fact]
        public void CreateAssetUrlReturnsUrlWithTheAssetPath()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/test/asset.coffee");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });

            var url = UrlGenerator.CreateAssetUrl(asset.Object);

            url.ShouldEqual("_cassette/asset/test/asset_coffee?01020f10");
        }

        [Fact]
        public void CreateAssetUrlWherePathIsInRootDirectoryReturnsUrlWithTheAssetPath()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/asset.coffee");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });

            var url = UrlGenerator.CreateAssetUrl(asset.Object);

            url.ShouldEqual("_cassette/asset/asset_coffee?01020f10");
        }

        [Fact]
        public void CreateAssetUrlWherePathHasNoExtensionReturnsUrlWithTheAssetPath()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/test/asset");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });

            var url = UrlGenerator.CreateAssetUrl(asset.Object);

            url.ShouldEqual("_cassette/asset/test/asset?01020f10");
        }

        [Fact]
        public void CreateAssetUrlWherePathHasPeriodsInDirectoryNamesReturnsUrlWithTheAssetPath()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/test.test/asset.coffee");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });

            var url = UrlGenerator.CreateAssetUrl(asset.Object);

            url.ShouldEqual("_cassette/asset/test.test/asset_coffee?01020f10");
        }
    }
}