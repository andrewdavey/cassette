using Moq;
using Xunit;

namespace Cassette.ModuleProcessing
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
        public void ProcessAddsAssetMinifierToAssetInModule()
        {
            var module = new Module("", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            module.Assets.Add(asset.Object);

            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddAssetTransformer(minifier.Object));
        }
    }
}
