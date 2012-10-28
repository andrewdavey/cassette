using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class AssignHash_Tests
    {
        readonly AssignHash assignHash;
        readonly TestableBundle bundle;

        public AssignHash_Tests()
        {
            assignHash = new AssignHash();
            bundle = new TestableBundle("~");
        }

        [Fact]
        public void GivenBundleHasNoAssetsThenBundleHashIsSHA1OfEmptyStream()
        {
            ProcessBundleWithAssignHash();
            AssertHashIsSha1Of("");
        }

        [Fact]
        public void GivenBundleHasOneAssetThenBundleHashIsSHA1OfAssetContent()
        {
            var asset = new StubAsset(content: "content");
            bundle.Assets.Add(asset);

            ProcessBundleWithAssignHash();

            AssertHashIsSha1Of("content");
        }

        [Fact]
        public void GivenBundleHasOneAssetThatContainsTwoAssetsThenBundleHashIsSHA1OfTheTwoChildAssets()
        {
            var childAsset1 = new StubAsset(content: "asset-1");
            var childAsset2 = new StubAsset(content: "asset-2");
            var combinedAsset = CombinedAsset(childAsset1, childAsset2);
            bundle.Assets.Add(combinedAsset);

            ProcessBundleWithAssignHash();

            AssertHashIsSha1Of("asset-1asset-2");
        }

        IAsset CombinedAsset(IAsset childAsset1, IAsset childAsset2)
        {
            var combinedAsset = new Mock<IAsset>();
            combinedAsset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                .Callback<IBundleVisitor>(v =>
                {
                    childAsset1.Accept(v);
                    childAsset2.Accept(v);
                });
            return combinedAsset.Object;
        }

        void ProcessBundleWithAssignHash()
        {
            assignHash.Process(bundle);
        }

        void AssertHashIsSha1Of(string expected)
        {
            bundle.Hash.ShouldEqual(expected.ComputeSHA1Hash());
        }
    }
}