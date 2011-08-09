using Moq;
using Should;
using Xunit;

namespace Cassette.ModuleProcessing
{
    public class SortAssetsByDependency_Tests
    {
        [Fact]
        public void GivenTwoAssetsWhereADependsOnB_WhenSorted_ThenBIsBeforeAInModule()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.SourceFilename).Returns("a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("test\\b.js", assetA.Object, 1, AssetReferenceType.SameModule) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.SourceFilename).Returns("b.js");
            module.Assets.Add(assetA.Object);
            module.Assets.Add(assetB.Object);

            var sorter = new SortAssetsByDependency();
            sorter.Process(module);

            module.Assets[0].ShouldBeSameAs(assetB.Object);
            module.Assets[1].ShouldBeSameAs(assetA.Object);
        }

        [Fact]
        public void GivenTwoAssetsWhereADependsOnBByDifferentlyCasedFilename_WhenSorted_ThenBIsBeforeAInModule()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.SourceFilename).Returns("a.js");
            assetA.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("TEST\\B.js", assetA.Object, 1, AssetReferenceType.SameModule) });
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.SourceFilename).Returns("b.js");
            module.Assets.Add(assetA.Object);
            module.Assets.Add(assetB.Object);

            var sorter = new SortAssetsByDependency();
            sorter.Process(module);

            module.Assets[0].ShouldBeSameAs(assetB.Object);
            module.Assets[1].ShouldBeSameAs(assetA.Object);
        }

        [Fact]
        public void GivenNoAssetReferences_WhenSorted_ThenAssetsOrderedBySourceFilename()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var assetA = new Mock<IAsset>();
            assetA.SetupGet(a => a.SourceFilename).Returns("a.js");
            var assetB = new Mock<IAsset>();
            assetB.SetupGet(a => a.SourceFilename).Returns("b.js");
            var assetC = new Mock<IAsset>();
            assetC.SetupGet(a => a.SourceFilename).Returns("c.js");
            // Add in wrong order here, to make sort have to do it's job.
            module.Assets.Add(assetB.Object);
            module.Assets.Add(assetC.Object);
            module.Assets.Add(assetA.Object);

            var sorter = new SortAssetsByDependency();
            sorter.Process(module);

            module.Assets[0].ShouldBeSameAs(assetA.Object);
            module.Assets[1].ShouldBeSameAs(assetB.Object);
            module.Assets[2].ShouldBeSameAs(assetC.Object);
        }
    }
}
