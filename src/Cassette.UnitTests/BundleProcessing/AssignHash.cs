using System;
using System.Security.Cryptography;
using Cassette.Configuration;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.BundleProcessing
{
    public class AssignHash_Tests : IDisposable
    {
        readonly AssignHash assignHash;
        readonly TestableBundle bundle;
        readonly SHA1 sha1;

        public AssignHash_Tests()
        {
            assignHash = new AssignHash();
            bundle = new TestableBundle("~");
            sha1 = SHA1.Create();
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
            var asset = StubAsset("content");
            bundle.Assets.Add(asset);

            ProcessBundleWithAssignHash();

            AssertHashIsSha1Of("content");
        }

        [Fact]
        public void GivenBundleHasOneAssetThatContainsTwoAssetsThenBundleHashIsSHA1OfTheTwoChildAssets()
        {
            var childAsset1 = StubAsset("asset-1");
            var childAsset2 = StubAsset("asset-2");
            var combinedAsset = CombinedAsset(childAsset1, childAsset2);
            bundle.Assets.Add(combinedAsset);

            ProcessBundleWithAssignHash();

            AssertHashIsSha1Of("asset-1asset-2");
        }

        IAsset StubAsset(string content)
        {
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.OpenStream()).Returns(() => content.AsStream());
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            return asset.Object;
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
            assignHash.Process(bundle, new CassetteSettings(""));
        }

        void AssertHashIsSha1Of(string expected)
        {
            using (var stream = expected.AsStream())
            {
                bundle.Hash.ShouldEqual(sha1.ComputeHash(stream));
            }
        }

        void IDisposable.Dispose()
        {
           sha1.Dispose();
        }
    }
}