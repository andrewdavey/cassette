#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

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
            var compiler = new Mock<ICompiler>();
            step = new CompileCoffeeScript(compiler.Object);
        }

        readonly CompileCoffeeScript step;

        [Fact]
        public void WhenProcessBundleContainingCoffeeScriptAsset_ThenCompileCoffeeScriptAssetTransformIsAddedToAsset()
        {
            var bundle = new TestableBundle("~");
            var coffeeScriptAsset = new Mock<IAsset>();
            coffeeScriptAsset.SetupGet(a => a.SourceFile.FullPath).Returns("test.coffee");
            bundle.Assets.Add(coffeeScriptAsset.Object);

            step.Process(bundle, new CassetteSettings());

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
            coffeeScriptAsset.SetupGet(a => a.SourceFile.FullPath).Returns("test.js");
            bundle.Assets.Add(coffeeScriptAsset.Object);

            step.Process(bundle, new CassetteSettings());

            coffeeScriptAsset.Verify(
                a => a.AddAssetTransformer(
                    It.IsAny<IAssetTransformer>()
                ),
                Times.Never()
            );
        }
    }
}

