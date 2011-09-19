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

using Moq;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExpandCssUrls_Tests
    {
        [Fact]
        public void ProcessAddsExpandCssUrlsAssetTransformerToEachAsset()
        {
            var processor = new ExpandCssUrls();
            var module = new StylesheetModule("~");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            module.Assets.Add(asset1.Object);
            module.Assets.Add(asset2.Object);

            processor.Process(module, Mock.Of<ICassetteApplication>());

            asset1.Verify(a => a.AddAssetTransformer(
                It.Is<IAssetTransformer>(t => t is ExpandCssUrlsAssetTransformer)
            ));
            asset2.Verify(a => a.AddAssetTransformer(
                It.Is<IAssetTransformer>(t => t is ExpandCssUrlsAssetTransformer)
            ));
        }
    }
}

