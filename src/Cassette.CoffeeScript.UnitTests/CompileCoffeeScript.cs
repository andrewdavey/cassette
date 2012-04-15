using Cassette.BundleProcessing;
using Cassette.Configuration;
using Moq;
using Xunit;

namespace Cassette.Scripts
{
    public class CompileCoffeeScript_Tests
    {
        public CompileCoffeeScript_Tests()
        {
            var compiler = new Mock<ICoffeeScriptCompiler>();
            step = new CompileCoffeeScript(compiler.Object, new CassetteSettings());
        }

        readonly CompileCoffeeScript step;

        [Fact]
        public void WhenProcessBundleContainingCoffeeScriptAsset_ThenCompileCoffeeScriptAssetTransformIsAddedToAsset()
        {
            var bundle = new TestableBundle("~");
            var coffeeScriptAsset = new Mock<IAsset>();
            coffeeScriptAsset.SetupGet(a => a.Path).Returns("test.coffee");
            bundle.Assets.Add(coffeeScriptAsset.Object);

            step.Process(bundle);

            coffeeScriptAsset.Verify(
                a => a.AddAssetTransformer(
                    It.Is<IAssetTransformer>(
                        t => t is CompileAsset
                    )
                )
            );
        }

        [Fact]
        public void WhenProcessBundleContainingJavaScriptAsset_ThenNoTransformsAreAddedToAsset()
        {
            var bundle = new TestableBundle("~");
            var coffeeScriptAsset = new Mock<IAsset>();
            coffeeScriptAsset.SetupGet(a => a.Path).Returns("test.js");
            bundle.Assets.Add(coffeeScriptAsset.Object);

            step.Process(bundle);

            coffeeScriptAsset.Verify(
                a => a.AddAssetTransformer(
                    It.IsAny<IAssetTransformer>()
                ),
                Times.Never()
            );
        }
    }
}

