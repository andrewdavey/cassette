using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using Cassette.CoffeeScript;
using Xunit;
using System.IO;

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
        public void WhenProcessModuleContainingCoffeeScriptAsset_ThenCoffeeScriptCompilerTransformIsAddedToAsset()
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

    }
}
