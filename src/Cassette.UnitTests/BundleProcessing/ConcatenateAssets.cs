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
            asset1.Setup(a => a.OpenStream()).Returns(() => ("asset1" + Environment.NewLine + "content").AsStream());
            asset2.Setup(a => a.OpenStream()).Returns(() => ("asset2" + Environment.NewLine + "content").AsStream());
            bundle.Assets.Add(asset1.Object);
            bundle.Assets.Add(asset2.Object);

            var processor = new ConcatenateAssets();
            processor.Process(bundle);

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
            var bundle = new TestableBundle("~");
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
            processor.Process(bundle);

            bundle.Assets[0].References
                .Select(r => r.Path)
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
            asset1.Setup(a => a.OpenStream()).Returns(() => "asset1".AsStream());
            asset2.Setup(a => a.OpenStream()).Returns(() => "asset2".AsStream());
            bundle.Assets.Add(asset1.Object);
            bundle.Assets.Add(asset2.Object);

            var processor = new ConcatenateAssets { Separator = ";" };
            processor.Process(bundle);

            using (var reader = new StreamReader(bundle.Assets[0].OpenStream()))
            {
                reader.ReadToEnd().ShouldEqual("asset1;asset2");
            }
            (bundle.Assets[0] as IDisposable).Dispose();
        }
    }
}