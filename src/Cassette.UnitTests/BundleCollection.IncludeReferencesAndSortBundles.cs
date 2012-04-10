using System;
using System.Collections.Generic;
using System.Linq;
using Cassette.Configuration;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleCollection_IncludeReferencesAndSortBundles
    {
        [Fact]
        public void GivenDiamondReferencing_ThenConcatDependenciesReturnsEachReferencedBundleOnlyOnceInDependencyOrder()
        {
            var bundle1 = new TestableBundle("~/bundle-1");
            var asset1 = new Mock<IAsset>();
            SetupAsset("~/bundle-1/a.js", asset1);
            asset1.SetupGet(a => a.References)
                  .Returns(new[] { 
                      new AssetReference("~/bundle-2/b.js", asset1.Object, 1, AssetReferenceType.DifferentBundle),
                      new AssetReference("~/bundle-3/c.js", asset1.Object, 1, AssetReferenceType.DifferentBundle)
                  });
            bundle1.Assets.Add(asset1.Object);

            var bundle2 = new TestableBundle("~/bundle-2");
            var asset2 = new Mock<IAsset>();
            SetupAsset("~/bundle-2/b.js", asset2);
            asset2.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/bundle-4/d.js", asset2.Object, 1, AssetReferenceType.DifferentBundle) });
            bundle2.Assets.Add(asset2.Object);

            var bundle3 = new TestableBundle("~/bundle-3");
            var asset3 = new Mock<IAsset>();
            SetupAsset("~/bundle-3/c.js", asset3);
            asset3.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~/bundle-4/d.js", asset3.Object, 1, AssetReferenceType.DifferentBundle) });
            bundle3.Assets.Add(asset3.Object);

            var bundle4 = new TestableBundle("~/bundle-4");
            var asset4 = new Mock<IAsset>();
            SetupAsset("~/bundle-4/d.js", asset4);
            bundle4.Assets.Add(asset4.Object);

            var collection = CreateBundleCollection(new[] { bundle1, bundle2, bundle3, bundle4 });

            collection.IncludeReferencesAndSortBundles(new[] { bundle1, bundle2, bundle3, bundle4 })
                .SequenceEqual(new[] { bundle4, bundle2, bundle3, bundle1 }).ShouldBeTrue();
        }

        [Fact]
        public void SortBundlesToleratesExternalBundlesWhichAreNotInTheContainer()
        {
            var externalBundle1 = new ExternalScriptBundle("http://test.com/test1.js");
            var externalBundle2 = new ExternalScriptBundle("http://test.com/test2.js");
            var collection = CreateBundleCollection(Enumerable.Empty<Bundle>());
            var results = collection.IncludeReferencesAndSortBundles(new[] { externalBundle1, externalBundle2 });
            results.SequenceEqual(new[] { externalBundle1, externalBundle2 }).ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleWithReferenceToAnotherBundle_BundlesAreSortedInDependencyOrder()
        {
            var bundle1 = new TestableBundle("~/bundle1");
            var bundle2 = new TestableBundle("~/bundle2");
            bundle1.AddReference("~/bundle2");

            var collection = CreateBundleCollection(new[] { bundle1, bundle2 });
            var sorted = collection.IncludeReferencesAndSortBundles(new[] { bundle1, bundle2 });
            sorted.SequenceEqual(new[] { bundle2, bundle1 }).ShouldBeTrue();
        }

        [Fact]
        public void GivenBundlesWithNoDependenciesAreReferencedInNonAlphaOrder_WhenIncludeReferencesAndSortBundles_ThenReferenceOrderIsMaintained()
        {
            var bundle1 = new TestableBundle("~/bundle1");
            var bundle2 = new TestableBundle("~/bundle2");
            var collection = CreateBundleCollection(new[] { bundle1, bundle2 });

            var sorted = collection.IncludeReferencesAndSortBundles(new[] { bundle2, bundle1 });

            sorted.SequenceEqual(new[] { bundle2, bundle1 }).ShouldBeTrue();
        }

        [Fact]
        public void GivenBundlesWithCyclicReferences_WhenIncludeReferencesAndSortBundles_ThenExceptionThrown()
        {
            var bundle1 = new TestableBundle("~/bundle1");
            var bundle2 = new TestableBundle("~/bundle2");
            bundle1.AddReference("~/bundle2");
            bundle2.AddReference("~/bundle1");
            var collection = CreateBundleCollection(new[] { bundle1, bundle2 });

            Assert.Throws<InvalidOperationException>(delegate
            {
                collection.IncludeReferencesAndSortBundles(new[] { bundle2, bundle1 });
            });
        }

        [Fact]
        public void ImplicitReferenceOrderingMustNotIntroduceCycles()
        {
            var ms = Enumerable.Range(0, 5).Select(i => new TestableBundle("~/" + i)).ToArray();

            ms[1].AddReference("~/4");
            ms[4].AddReference("~/3");

            var collection = CreateBundleCollection(ms);
            var sorted = collection.IncludeReferencesAndSortBundles(ms).ToArray();

            sorted[0].ShouldBeSameAs(ms[0]);
            sorted[1].ShouldBeSameAs(ms[2]);
            sorted[2].ShouldBeSameAs(ms[3]);
            sorted[3].ShouldBeSameAs(ms[4]);
            sorted[4].ShouldBeSameAs(ms[1]);
        }

        [Fact]
        public void WhenSortBundlesWithCycle_ThenExceptionThrownHasMessageWithCycleBundlePaths()
        {
            var bundleA = new TestableBundle("~/a");
            var bundleB = new TestableBundle("~/b");
            bundleA.AddReference("~/b");
            bundleB.AddReference("~/a");

            var collection = CreateBundleCollection(new[] { bundleA, bundleB });
            var exception = Assert.Throws<InvalidOperationException>(
                () => collection.IncludeReferencesAndSortBundles(new[] { bundleA, bundleB })
            );
            exception.Message.ShouldContain("~/a");
            exception.Message.ShouldContain("~/b");
        }

        BundleCollection CreateBundleCollection(IEnumerable<Bundle> bundles)
        {
            var collection = new BundleCollection(new CassetteSettings(""), t => null, Mock.Of<IBundleFactoryProvider>());
            foreach (var bundle in bundles)
            {
                collection.Add(bundle);
            }
            return collection;
        }

        void SetupAsset(string filename, Mock<IAsset> asset)
        {
            asset.Setup(a => a.Path).Returns(filename);
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
        }
    }
}