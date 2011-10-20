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
using Moq;
using Should;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class SortAssetsByDependency_Tests
    {
        [Fact]
        public void GivenTwoAssetsWhereADependsOnB_WhenSorted_ThenBIsBeforeAInBundle()
        {
            var bundle = new TestableBundle("~/test");
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/test/b.js", assetA.Object, 1, AssetReferenceType.SameBundle) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/b.js");
            bundle.Assets.Add(assetA.Object);
            bundle.Assets.Add(assetB.Object);

            var sorter = new SortAssetsByDependency();
            sorter.Process(bundle, Mock.Of<ICassetteApplication>());

            bundle.Assets[0].ShouldBeSameAs(assetB.Object);
            bundle.Assets[1].ShouldBeSameAs(assetA.Object);
        }

        [Fact]
        public void GivenTwoAssetsWhereADependsOnBByDifferentlyCasedFilename_WhenSorted_ThenBIsBeforeAInBundle()
        {
            var bundle = new TestableBundle("~/test");
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/TEST/B.js", assetA.Object, 1, AssetReferenceType.SameBundle) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/b.js");
            bundle.Assets.Add(assetA.Object);
            bundle.Assets.Add(assetB.Object);

            var sorter = new SortAssetsByDependency();
            sorter.Process(bundle, Mock.Of<ICassetteApplication>());

            bundle.Assets[0].ShouldBeSameAs(assetB.Object);
            bundle.Assets[1].ShouldBeSameAs(assetA.Object);
        }

        [Fact]
        public void WhenBundleHasSortedAssets_ThenProcessDoesNotReorderAssets()
        {
            var bundle = new TestableBundle("~/test");
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/TEST/B.js", assetA.Object, 1, AssetReferenceType.SameBundle) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/b.js");
            bundle.AddAssets(new[] {assetA.Object, assetB.Object}, preSorted: true);
            
            var sorter = new SortAssetsByDependency();
            sorter.Process(bundle, Mock.Of<ICassetteApplication>());

            bundle.Assets[0].ShouldBeSameAs(assetA.Object);
            bundle.Assets[1].ShouldBeSameAs(assetB.Object);
        }

        [Fact]
        public void GivenAssetWithCircularReferences_WhenProcess_ThenExceptionThrown()
        {
            var bundle = new TestableBundle("~/test");
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/test/b.js", assetA.Object, 1, AssetReferenceType.SameBundle) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/b.js");
            assetB.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/test/a.js", assetB.Object, 1, AssetReferenceType.SameBundle) });

            bundle.AddAssets(new[] { assetA.Object, assetB.Object }, preSorted: false);

            var sorter = new SortAssetsByDependency();
            Assert.Throws<InvalidOperationException>(
                () => sorter.Process(bundle, Mock.Of<ICassetteApplication>())
            );
        }
    }
}

