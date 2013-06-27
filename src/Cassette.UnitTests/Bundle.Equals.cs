using Cassette.BundleProcessing;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleEqualityTests
    {
        [Fact]
        public void BundlesWithSamePathAreEqual()
        {
            var bundle1 = new TestableBundle("~/bundle");
            var bundle2 = new TestableBundle("~/bundle");
            bundle1.Equals(bundle2).ShouldBeTrue();
        }

        [Fact]
        public void BundlesWithDifferentPathsAreNotEqual()
        {
            var bundle1 = new TestableBundle("~/bundle1");
            var bundle2 = new TestableBundle("~/bundle2");
            bundle1.Equals(bundle2).ShouldBeFalse();
        }

        [Fact]
        public void BundlesOfDifferentTypesAreNotEqual()
        {
            Bundle bundle1 = new ScriptBundle("~");
            Bundle bundle2 = new StylesheetBundle("~");
            bundle1.Equals(bundle2).ShouldBeFalse();
        }

        [Fact]
        public void BundlesWithSamePathButDifferentAssetsAreNotEqual()
        {
            var bundle1 = new TestableBundle("~/bundle");
            var asset1 = new StubAsset("~/bundle/asset1.js");
            bundle1.Assets.Add(asset1);
            var bundle2 = new TestableBundle("~/bundle");
            var asset2 = new StubAsset("~/bundle/asset2.js");
            bundle2.Assets.Add(asset2);
            bundle1.Equals(bundle2).ShouldBeFalse();
        }

        [Fact]
        public void BundleAssetsAreSortedByPathBeforeBeingComparedForEquality()
        {
            var bundle1 = new TestableBundle("~/bundle");
            bundle1.Assets.Add(new StubAsset("~/bundle/asset1.js"));
            bundle1.Assets.Add(new StubAsset("~/bundle/asset2.js"));

            var bundle2 = new TestableBundle("~/bundle");
            bundle2.Assets.Add(new StubAsset("~/bundle/asset2.js"));
            bundle2.Assets.Add(new StubAsset("~/bundle/asset1.js"));

            bundle1.Equals(bundle2).ShouldBeTrue();
        }

        [Fact]
        public void BundleWithConcatenatedAssetsEqualsBundleWithUnconcatenatedAssets()
        {
            var bundle1 = new TestableBundle("~/bundle");
            bundle1.Assets.Add(
                new ConcatenatedAsset(
                    "~/bundle",
                    new[] { new StubAsset("~/bundle/asset1.js"), new StubAsset("~/bundle/asset2.js") },
                    ";"
                )
            );

            var bundle2 = new TestableBundle("~/bundle");
            bundle2.Assets.Add(new StubAsset("~/bundle/asset2.js"));
            bundle2.Assets.Add(new StubAsset("~/bundle/asset1.js"));

            bundle1.Equals(bundle2).ShouldBeTrue();
        }
    }
}