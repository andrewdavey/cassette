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
            assetA.SetupGet(a => a.Path).Returns("~/test/a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/test/b.js", assetA.Object, 1, AssetReferenceType.SameBundle) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.Path).Returns("~/test/b.js");
            bundle.Assets.Add(assetA.Object);
            bundle.Assets.Add(assetB.Object);

            var sorter = new SortAssetsByDependency();
            sorter.Process(bundle);

            bundle.Assets[0].ShouldBeSameAs(assetB.Object);
            bundle.Assets[1].ShouldBeSameAs(assetA.Object);
        }

        [Fact]
        public void GivenTwoAssetsWhereADependsOnBByDifferentlyCasedFilename_WhenSorted_ThenBIsBeforeAInBundle()
        {
            var bundle = new TestableBundle("~/test");
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.Path).Returns("~/test/a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/TEST/B.js", assetA.Object, 1, AssetReferenceType.SameBundle) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.Path).Returns("~/test/b.js");
            bundle.Assets.Add(assetA.Object);
            bundle.Assets.Add(assetB.Object);

            var sorter = new SortAssetsByDependency();
            sorter.Process(bundle);

            bundle.Assets[0].ShouldBeSameAs(assetB.Object);
            bundle.Assets[1].ShouldBeSameAs(assetA.Object);
        }

        [Fact]
        public void WhenBundleHasSortedAssets_ThenProcessDoesNotReorderAssets()
        {
            var bundle = new TestableBundle("~/test");
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.Path).Returns("~/test/a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/TEST/B.js", assetA.Object, 1, AssetReferenceType.SameBundle) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.Path).Returns("~/test/b.js");
            bundle.Assets.Add(assetA.Object);
            bundle.Assets.Add(assetB.Object);
            bundle.IsSorted = true;

            var sorter = new SortAssetsByDependency();
            sorter.Process(bundle);

            bundle.Assets[0].ShouldBeSameAs(assetA.Object);
            bundle.Assets[1].ShouldBeSameAs(assetB.Object);
        }

        [Fact]
        public void GivenAssetWithCircularReferences_WhenProcess_ThenExceptionThrown()
        {
            var bundle = new TestableBundle("~/test");
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.Path).Returns("~/test/a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/test/b.js", assetA.Object, 1, AssetReferenceType.SameBundle) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.Path).Returns("~/test/b.js");
            assetB.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/test/a.js", assetB.Object, 1, AssetReferenceType.SameBundle) });
            
            bundle.Assets.Add(assetA.Object);
            bundle.Assets.Add(assetB.Object);

            var sorter = new SortAssetsByDependency();
            Assert.Throws<InvalidOperationException>(
                () => sorter.Process(bundle)
            );
        }
    }
}

