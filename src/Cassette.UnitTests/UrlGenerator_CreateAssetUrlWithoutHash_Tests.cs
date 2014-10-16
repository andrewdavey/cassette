using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class UrlGenerator_CreateAssetUrlWithoutHash_Tests : UrlGeneratorTestsBase
    {
        readonly Mock<IAsset> asset;

        public UrlGenerator_CreateAssetUrlWithoutHash_Tests() : base(true)
        {
            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });
        }

        [Fact]
        public void UrlModifierModifyIsCalled()
        {
            asset.SetupGet(a => a.Path).Returns("~/test/asset.coffee");

            UrlGenerator.CreateAssetUrl(asset.Object);

            UrlModifier.Verify(m => m.Modify(It.IsAny<string>()));
        }

        [Fact]
        public void CreateAssetUrlReturnsUrlWithTheAssetPath()
        {
            asset.SetupGet(a => a.Path).Returns("~/test/asset.coffee");

            var url = UrlGenerator.CreateAssetUrl(asset.Object);

            url.ShouldEqual("cassette.axd/asset/test/asset.coffee");
        }

        [Fact]
        public void CreateAssetUrlWherePathIsInRootDirectoryReturnsUrlWithTheAssetPath()
        {
            asset.SetupGet(a => a.Path).Returns("~/asset.coffee");

            var url = UrlGenerator.CreateAssetUrl(asset.Object);

            url.ShouldEqual("cassette.axd/asset/asset.coffee");
        }

        [Fact]
        public void CreateAssetUrlWherePathHasNoExtensionReturnsUrlWithTheAssetPath()
        {
            asset.SetupGet(a => a.Path).Returns("~/test/asset");

            var url = UrlGenerator.CreateAssetUrl(asset.Object);

            url.ShouldEqual("cassette.axd/asset/test/asset");
        }

        [Fact]
        public void CreateAssetUrlWherePathHasPeriodsInDirectoryNamesReturnsUrlWithTheAssetPath()
        {
            asset.SetupGet(a => a.Path).Returns("~/test.test/asset.coffee");

            var url = UrlGenerator.CreateAssetUrl(asset.Object);

            url.ShouldEqual("cassette.axd/asset/test.test/asset.coffee");
        }
    }
}