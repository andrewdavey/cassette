using Cassette.Configuration;
using Moq;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExpandCssUrls_Tests
    {
        [Fact]
        public void ProcessAddsExpandCssUrlsAssetTransformerToEachAsset()
        {
            var processor = new ExpandCssUrls(Mock.Of<IUrlGenerator>(), new CassetteSettings());
            var bundle = new StylesheetBundle("~");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            bundle.Assets.Add(asset1.Object);
            bundle.Assets.Add(asset2.Object);

            processor.Process(bundle);

            asset1.Verify(a => a.AddAssetTransformer(
                It.Is<IAssetTransformer>(t => t is ExpandCssUrlsAssetTransformer)
            ));
            asset2.Verify(a => a.AddAssetTransformer(
                It.Is<IAssetTransformer>(t => t is ExpandCssUrlsAssetTransformer)
            ));
        }
    }
}