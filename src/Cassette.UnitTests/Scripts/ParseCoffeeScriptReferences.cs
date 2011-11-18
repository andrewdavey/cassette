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

using Cassette.Utilities;
using Moq;
using Xunit;

namespace Cassette.Scripts
{
    public class ParseCoffeeScriptReferences_Tests
    {
        [Fact]
        public void ProcessAddsReferencesToCoffeeScriptAssetInBundle()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/asset.coffee");

            var coffeeScriptSource = @"
# @reference ""another1.js""
# @reference 'another2.coffee'
# @reference another3.coffee

class Foo
";
            asset.Setup(a => a.OpenStream())
                 .Returns(coffeeScriptSource.AsStream());
            var bundle = new ScriptBundle("~");
            bundle.Assets.Add(asset.Object);

            var processor = new ParseCoffeeScriptReferences();
            processor.Process(bundle, Mock.Of<ICassetteApplication>());

            asset.Verify(a => a.AddReference("another1.js", 2));
            asset.Verify(a => a.AddReference("another2.coffee", 3));
            asset.Verify(a => a.AddReference("another3.coffee", 4));
        }
    }
}

