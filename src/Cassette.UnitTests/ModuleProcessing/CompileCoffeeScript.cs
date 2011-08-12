using Cassette.Compilation;
using Moq;
using Xunit;

namespace Cassette.ModuleProcessing
{
    public class CompileCoffeeScript_Tests
    {
        public CompileCoffeeScript_Tests()
        {
            var compiler = new Mock<ICompiler>();
            step = new CompileCoffeeScript(compiler.Object);
        }

        readonly CompileCoffeeScript step;

        [Fact]
        public void WhenProcessModuleContainingCoffeeScriptAsset_ThenCompileCoffeeScriptAssetTransformIsAddedToAsset()
        {
            var module = new Module("", Mock.Of<IFileSystem>());
            var coffeeScriptAsset = new Mock<IAsset>();
            coffeeScriptAsset.SetupGet(a => a.SourceFilename).Returns("test.coffee");
            module.Assets.Add(coffeeScriptAsset.Object);

            step.Process(module, Mock.Of<ICassetteApplication>());

            coffeeScriptAsset.Verify(
                a => a.AddAssetTransformer(
                    It.Is<IAssetTransformer>(
                        t => t is CompileAsset
                    )
                )
            );
        }

        [Fact]
        public void WhenProcessModuleContainingJavaScriptAsset_ThenNoTransformsAreAddedToAsset()
        {
            var module = new Module("", Mock.Of<IFileSystem>());
            var coffeeScriptAsset = new Mock<IAsset>();
            coffeeScriptAsset.SetupGet(a => a.SourceFilename).Returns("test.js");
            module.Assets.Add(coffeeScriptAsset.Object);

            step.Process(module, Mock.Of<ICassetteApplication>());

            coffeeScriptAsset.Verify(
                a => a.AddAssetTransformer(
                    It.IsAny<IAssetTransformer>()
                ),
                Times.Never()
            );
        }
    }
}
