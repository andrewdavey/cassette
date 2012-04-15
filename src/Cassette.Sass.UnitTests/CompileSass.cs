#if !NET35
using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using Xunit;

namespace Cassette.Stylesheets
{
    public class CompileSass_Tests
    {
        readonly CompileSass processor;
        readonly StylesheetBundle bundle;
        readonly Mock<IAsset> asset;

        public CompileSass_Tests()
        {
            processor = new CompileSass(Mock.Of<ISassCompiler>(), new CassetteSettings());
            bundle = new StylesheetBundle("~");
            asset = new Mock<IAsset>();
        }

        [Fact]
        public void GivenACompiler_WhenProcessCalled_ThenCompileAssetTransformerAddedToScssAsset()
        {
            asset.SetupGet(a => a.Path).Returns("test.scss");
            bundle.Assets.Add(asset.Object);

            processor.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(at => at is CompileAsset)));
        }

        [Fact]
        public void GivenACompiler_WhenProcessCalled_ThenCompileAssetTransformerAddedToSassAsset()
        {
            asset.SetupGet(a => a.Path).Returns("test.sass");
            bundle.Assets.Add(asset.Object);

            processor.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(at => at is CompileAsset)));
        }

        [Fact]
        public void GivenACompiler_WhenProcessCalled_ThenCompileAssetTransformerNotAddedToCssAsset()
        {
            asset.SetupGet(a => a.Path).Returns("test.css");
            bundle.Assets.Add(asset.Object);

            processor.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(at => at is CompileAsset)), Times.Never());
        }
    }
}
#endif