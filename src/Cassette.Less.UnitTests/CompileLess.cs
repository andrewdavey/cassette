using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using Xunit;

namespace Cassette.Stylesheets
{
    public class CompileLess_Tests
    {
        [Fact]
        public void GivenACompiler_WhenProcessCalled_ThenCompileAssetTransformerAddedToLessAsset()
        {
            var processor = new CompileLess(Mock.Of<ICompiler>(), new CassetteSettings());
            var bundle = new StylesheetBundle("~");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("test.less");
            bundle.Assets.Add(asset.Object);

            processor.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(at => at is CompileAsset)));
        }

        [Fact]
        public void GivenACompiler_WhenProcessCalled_ThenCompileAssetTransformerNotAddedToCssAsset()
        {
            var processor = new CompileLess(Mock.Of<ICompiler>(), new CassetteSettings());
            var bundle = new StylesheetBundle("~");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("test.css");
            bundle.Assets.Add(asset.Object);

            processor.Process(bundle);

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(at => at is CompileAsset)), Times.Never());
        }
    }
}