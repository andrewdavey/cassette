using System;
using System.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class ConcatenateAssets_Tests
    {
        [Fact]
        public void GivenEmptyBundle_WhenConcatenateAssets_ThenNoAssetAddedToBundle()
        {
            var bundle = new TestableBundle("~");
            bundle.ConcatenateAssets("");
            bundle.Assets.Count.ShouldEqual(0);
        }

        [Fact]
        public void GivenBundleWithTwoAssets_WhenConcatenateAssetsProcessesBundle_ThenASingleAssetReplacesTheTwoOriginalAssets()
        {
            var bundle = new TestableBundle("~");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            asset1.Setup(a => a.GetTransformedContent()).Returns("asset1" + Environment.NewLine + "content");
            asset2.Setup(a => a.GetTransformedContent()).Returns("asset2" + Environment.NewLine + "content");
            bundle.Assets.Add(asset1.Object);
            bundle.Assets.Add(asset2.Object);

            var processor = new ConcatenateAssets();
            processor.Process(bundle);

            bundle.Assets.Count.ShouldEqual(1);
            bundle.Assets[0].GetTransformedContent()
                            .ShouldEqual(
                            "asset1" + Environment.NewLine + "content" + 
                            Environment.NewLine + 
                            "asset2" + Environment.NewLine + "content");
        }

        [Fact]
        public void ConcatenateAssetsMergesAssetReferences()
        {
            var bundle = new TestableBundle("~");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            asset1.Setup(a => a.GetTransformedContent()).Returns("asset1");
            asset1.SetupGet(a => a.References).Returns(new[] 
            {
                new AssetReference(asset1.Object.Path, "~\\other1.js", 0, AssetReferenceType.DifferentBundle)
            });
            asset2.Setup(a => a.GetTransformedContent()).Returns("asset2");
            asset2.SetupGet(a => a.References).Returns(new[]
            { 
                new AssetReference(asset2.Object.Path, "~\\other1.js", 0, AssetReferenceType.DifferentBundle),
                new AssetReference(asset2.Object.Path, "~\\other2.js", 0, AssetReferenceType.DifferentBundle) 
            });
            bundle.Assets.Add(asset1.Object);
            bundle.Assets.Add(asset2.Object);

            var processor = new ConcatenateAssets();
            processor.Process(bundle);

            bundle.Assets[0].References
                .Select(r => r.ToPath)
                .OrderBy(f => f)
                .SequenceEqual(new[] { "~\\other1.js", "~\\other1.js", "~\\other2.js" })
                .ShouldBeTrue();
        }

        [Fact]
        public void ConcatenateAssetsWithSeparatorPutsSeparatorBetweenEachAsset()
        {
            var bundle = new TestableBundle("~");
            var asset1 = new Mock<IAsset>();
            var asset2 = new Mock<IAsset>();
            asset1.Setup(a => a.GetTransformedContent()).Returns("asset1");
            asset2.Setup(a => a.GetTransformedContent()).Returns("asset2");
            bundle.Assets.Add(asset1.Object);
            bundle.Assets.Add(asset2.Object);

            var processor = new ConcatenateAssets { Separator = ";" };
            processor.Process(bundle);

            bundle.Assets[0].GetTransformedContent().ShouldEqual("asset1;asset2");
        }
    }
}