using Moq;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExpandCssUrls_Tests
    {
        [Fact]
        public void ProcessAddsExpandCssUrlsAssetTransformerToEachAsset()
        {
            var processor = new ExpandCssUrls();
            var module = new StylesheetModule("~");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);

            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset1.Verify(a => a.AddAssetTransformer(
                It.Is<IAssetTransformer>(t => t is ExpandCssUrlsAssetTransformer)
            ));
            asset2.Verify(a => a.AddAssetTransformer(
                It.Is<IAssetTransformer>(t => t is ExpandCssUrlsAssetTransformer)
            ));
        }
    }
}
