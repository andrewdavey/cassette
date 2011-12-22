using Cassette.Configuration;
using Moq;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class MinifyAssets_Tests
    {
        public MinifyAssets_Tests()
        {
            minifier = new Mock<IAssetTransformer>();
            processor = new MinifyAssets(minifier.Object);
        }

        readonly MinifyAssets processor;
        readonly Mock<IAssetTransformer> minifier;

        [Fact]
        public void ProcessAddsAssetMinifierToAssetInBundle()
        {
            var bundle = new TestableBundle("~");
            var asset = new Mock<IAsset>();
            bundle.Assets.Add(asset.Object);

            processor.Process(bundle, new CassetteSettings(""));

            asset.Verify(a => a.AddAssetTransformer(minifier.Object));
        }
    }
}

