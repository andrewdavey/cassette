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

using System;
using System.IO;
using System.Linq;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class ConcatenateAssets_Tests
    {
        [Fact]
        public void GivenBundleWithTwoAssets_WhenConcatenateAssetsProcessesBundle_ThenASingleAssetReplacesTheTwoOriginalAssets()
        {
            var bundle = new Bundle("~");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            asset1.Setup(a => a.OpenStream()).Returns(() => ("asset1" + Environment.NewLine + "content").AsStream());
            asset2.Setup(a => a.OpenStream()).Returns(() => ("asset2" + Environment.NewLine + "content").AsStream());
            bundle.Assets.Add(asset1.Object);
            bundle.Assets.Add(asset2.Object);

            var processor = new ConcatenateAssets();
            processor.Process(bundle, Mock.Of<ICassetteApplication>());

            bundle.Assets.Count.ShouldEqual(1);
            using (var reader = new StreamReader(bundle.Assets[0].OpenStream()))
            {
                reader.ReadToEnd().ShouldEqual("asset1" + Environment.NewLine + "content" + Environment.NewLine + "asset2" + Environment.NewLine + "content");
            }
            (bundle.Assets[0] as IDisposable).Dispose();
        }

        [Fact]
        public void ConcatenateAssetsMergesAssetReferences()
        {
            var bundle = new Bundle("~");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            asset1.Setup(a => a.OpenStream()).Returns(() => "asset1".AsStream());
            asset1.SetupGet(a => a.References).Returns(new[] 
            {
                new AssetReference("~\\other1.js", asset1.Object, 0, AssetReferenceType.DifferentBundle)
            });
            asset2.Setup(a => a.OpenStream()).Returns(() => "asset2".AsStream());
            asset2.SetupGet(a => a.References).Returns(new[]
            { 
                new AssetReference("~\\other1.js", asset2.Object, 0, AssetReferenceType.DifferentBundle),
                new AssetReference("~\\other2.js", asset2.Object, 0, AssetReferenceType.DifferentBundle) 
            });
            bundle.Assets.Add(asset1.Object);
            bundle.Assets.Add(asset2.Object);

            var processor = new ConcatenateAssets();
            processor.Process(bundle, Mock.Of<ICassetteApplication>());

            bundle.Assets[0].References
                .Select(r => r.Path)
                .OrderBy(f => f)
                .SequenceEqual(new[] { "~\\other1.js", "~\\other1.js", "~\\other2.js" })
                .ShouldBeTrue();
        }
    }
}

