using Cassette.CoffeeScript;
using Moq;
using Xunit;

namespace Cassette
{
    public class CompileCoffeeScript_Tests
    {
        public CompileCoffeeScript_Tests()
        {
            var compiler = new Mock<ICoffeeScriptCompiler>();
            step = new CompileCoffeeScript(compiler.Object);
        }

        readonly CompileCoffeeScript step;

        [Fact]
        public void WhenProcessModuleContainingCoffeeScriptAsset_ThenCompileCoffeeScriptAssetTransformIsAddedToAsset()
        {
            var module = new Module("c:\\");
            var coffeeScriptAsset = new Mock<IAsset>();
            coffeeScriptAsset.SetupGet(a => a.SourceFilename).Returns("c:\\test.coffee");
            module.Assets.Add(coffeeScriptAsset.Object);

            step.Process(module);

            coffeeScriptAsset.Verify(
                a => a.AddAssetTransformer(
                    It.Is<IAssetTransformer>(
                        t => t is CompileCoffeeScriptAsset
                    )
                )
            );
        }

        [Fact]
        public void WhenProcessModuleContainingJavaScriptAsset_ThenNoTransformsAreAddedToAsset()
        {
            var module = new Module("c:\\");
            var coffeeScriptAsset = new Mock<IAsset>();
            coffeeScriptAsset.SetupGet(a => a.SourceFilename).Returns("c:\\test.js");
            module.Assets.Add(coffeeScriptAsset.Object);

            step.Process(module);

            coffeeScriptAsset.Verify(
                a => a.AddAssetTransformer(
                    It.IsAny<IAssetTransformer>()
                ),
                Times.Never()
            );
        }
    }
}
