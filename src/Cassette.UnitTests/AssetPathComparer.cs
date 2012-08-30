using Should;
using Xunit;

namespace Cassette
{
    public class AssetPathComparer_Tests
    {
        readonly AssetPathComparer comparer = new AssetPathComparer();

        [Fact]
        public void AssetsWithSamePathAreEqual()
        {
            var asset1 = new StubAsset("~/test-asset.js");
            var asset2 = new StubAsset("~/test-asset.js");
            comparer.Equals(asset1, asset2).ShouldBeTrue();
        }

        [Fact]
        public void AssetsWithDifferentPathsAreNotEqual()
        {
            var asset1 = new StubAsset("~/test-asset1.js");
            var asset2 = new StubAsset("~/test-asset2.js");
            comparer.Equals(asset1, asset2).ShouldBeFalse();
        }
    }
}