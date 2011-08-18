using Cassette.ModuleProcessing;
using Moq;
using Xunit;

namespace Cassette.Stylesheets
{
    public class CompileLess_Tests
    {
        [Fact]
        public void GivenACompiler_WhenProcessCalled_ThenCompileAssetTransformerAddedToLessAsset()
        {
            var processor = new CompileLess(Mock.Of<ICompiler>());
            var module = new StylesheetModule("");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("test.less");
            module.Assets.Add(asset.Object);

            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(at => at is CompileAsset)));
        }

        [Fact]
        public void GivenACompiler_WhenProcessCalled_ThenCompileAssetTransformerNotAddedToCssAsset()
        {
            var processor = new CompileLess(Mock.Of<ICompiler>());
            var module = new StylesheetModule("");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("test.css");
            module.Assets.Add(asset.Object);

            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddAssetTransformer(It.Is<IAssetTransformer>(at => at is CompileAsset)), Times.Never());
        }
    }
}
