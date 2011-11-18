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

using Cassette.Configuration;
using Moq;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class MinifyAssets_Tests
    {
        public MinifyAssets_Tests()
        {
            minifier = new Mock<IAssetTransformer>();
            processor = new MinifyAssets(minifier.Object);
        }

        readonly MinifyAssets processor;
        readonly Mock<IAssetTransformer> minifier;

        [Fact]
        public void ProcessAddsAssetMinifierToAssetInBundle()
        {
            var bundle = new TestableBundle("~");
            var asset = new Mock<IAsset>();
            bundle.Assets.Add(asset.Object);

            processor.Process(bundle, new CassetteSettings());

            asset.Verify(a => a.AddAssetTransformer(minifier.Object));
        }
    }
}

