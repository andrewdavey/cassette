using System;
using System.Linq;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleContainer_Tests
    {
        [Fact]
        public void GivenAssetWithUnknownDifferentBundleReference_ThenConstructorThrowsAssetReferenceException()
        {
            var bundle = new TestableBundle("~/bundle-1");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("bundle-1\\a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~\\fail\\fail.js", asset.Object, 0, AssetReferenceType.DifferentBundle) });
            bundle.Assets.Add(asset.Object);

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                new BundleContainer(new[] { bundle });
            });
            exception.Message.ShouldEqual("Reference error in \"bundle-1\\a.js\". Cannot find \"~\\fail\\fail.js\".");
        }

        [Fact]
        public void GivenAssetWithUnknownDifferentBundleReferenceHavingLineNumber_ThenConstructorThrowsAssetReferenceException()
        {
            var bundle = new TestableBundle("~/bundle-1");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/bundle-1/a.js");
            asset.SetupGet(a => a.References)
                  .Returns(new[] { new AssetReference("~\\fail\\fail.js", asset.Object, 42, AssetReferenceType.DifferentBundle) });
            bundle.Assets.Add(asset.Object);

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                new BundleContainer(new[] { bundle });
            });
            exception.Message.ShouldEqual("Reference error in \"~/bundle-1/a.js\", line 42. Cannot find \"~\\fail\\fail.js\".");
        }

        [Fact]
        public void FindBundleContainingPathOfBundleReturnsTheBundle()
        {
            var expectedBundle = new TestableBundle("~/test");
            var container = new BundleContainer(new[] {
                expectedBundle
            });
            var actualBundle = container.FindBundleContainingPath<Bundle>("~/test");
            actualBundle.ShouldBeSameAs(expectedBundle);
        }

        [Fact]
        public void FindBundleContainingPathWithWrongPathReturnsNull()
        {
            var container = new BundleContainer(new[] {
                new TestableBundle("~/test")
            });
            var actualBundle = container.FindBundleContainingPath<Bundle>("~/WRONG");
            actualBundle.ShouldBeNull();
        }

        [Fact]
        public void FindBundleContainingPathOfAssetReturnsTheBundle()
        {
            var expectedBundle = new TestableBundle("~/test");
            var asset = new Mock<IAsset>();
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/test.js");
            expectedBundle.Assets.Add(asset.Object);
            var container = new BundleContainer(new[] {
                expectedBundle
            });
            var actualBundle = container.FindBundleContainingPath<Bundle>("~/test/test.js");
            actualBundle.ShouldBeSameAs(expectedBundle);
        }

        [Fact]
        public void GivenBundleWithInvalid_ConstructorThrowsException()
        {
            var bundle1 = new TestableBundle("~/bundle1");
            bundle1.AddReference("~\\bundle2");

            var exception = Assert.Throws<AssetReferenceException>(delegate
            {
                new BundleContainer(new[] {bundle1});
            });
            exception.Message.ShouldEqual("Reference error in bundle descriptor for \"~/bundle1\". Cannot find \"~/bundle2\".");
        }
    }

    public class BundleContainer_IncludeReferencesAndSortBundles_Tests
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

            var container = new BundleContainer(new[] { bundle1, bundle2, bundle3, bundle4 });

            container.IncludeReferencesAndSortBundles(new[] { bundle1, bundle2, bundle3, bundle4 })
                .SequenceEqual(new[] { bundle4, bundle2, bundle3, bundle1 }).ShouldBeTrue();
        }

        [Fact]
        public void SortBundlesToleratesExternalBundlesWhichAreNotInTheContainer()
        {
            var externalBundle1 = new ExternalScriptBundle("http://test.com/test1.js");
            var externalBundle2 = new ExternalScriptBundle("http://test.com/test2.js");
            var container = new BundleContainer(Enumerable.Empty<ScriptBundle>());
            var results = container.IncludeReferencesAndSortBundles(new[] { externalBundle1, externalBundle2 });
            results.SequenceEqual(new[] { externalBundle1, externalBundle2 }).ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleWithReferenceToAnotherBundle_BundlesAreSortedInDependencyOrder()
        {
            var bundle1 = new TestableBundle("~/bundle1");
            var bundle2 = new TestableBundle("~/bundle2");
            bundle1.AddReference("~/bundle2");

            var container = new BundleContainer(new[] { bundle1, bundle2 });
            var sorted = container.IncludeReferencesAndSortBundles(new[] { bundle1, bundle2 });
            sorted.SequenceEqual(new[] { bundle2, bundle1 }).ShouldBeTrue();
        }

        [Fact]
        public void GivenBundlesWithNoDependenciesAreReferencedInNonAlphaOrder_WhenIncludeReferencesAndSortBundles_ThenReferenceOrderIsMaintained()
        {
            var bundle1 = new TestableBundle("~/bundle1");
            var bundle2 = new TestableBundle("~/bundle2");
            var container = new BundleContainer(new[] { bundle1, bundle2 });
            
            var sorted = container.IncludeReferencesAndSortBundles(new[] { bundle2, bundle1 });

            sorted.SequenceEqual(new[] { bundle2, bundle1 }).ShouldBeTrue();
        }

        [Fact]
        public void GivenBundlesWithCyclicReferences_WhenIncludeReferencesAndSortBundles_ThenExceptionThrown()
        {
            var bundle1 = new TestableBundle("~/bundle1");
            var bundle2 = new TestableBundle("~/bundle2");
            bundle1.AddReference("~/bundle2");
            bundle2.AddReference("~/bundle1");
            var container = new BundleContainer(new[] { bundle1, bundle2 });

            Assert.Throws<InvalidOperationException>(delegate
            {
                container.IncludeReferencesAndSortBundles(new[] { bundle2, bundle1 });
            });
        }

        [Fact]
        public void ImplicitReferenceOrderingMustNotIntroduceCycles()
        {
            var ms = Enumerable.Range(0, 5).Select(i => new TestableBundle("~/" + i)).ToArray();

            ms[1].AddReference("~/4");
            ms[4].AddReference("~/3");

            var container = new BundleContainer(ms);
            var sorted = container.IncludeReferencesAndSortBundles(ms).ToArray();

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

            var container = new BundleContainer(new[] { bundleA, bundleB });
            var exception = Assert.Throws<InvalidOperationException>(
                () => container.IncludeReferencesAndSortBundles(new[] { bundleA, bundleB })
            );
            exception.Message.ShouldContain("~/a");
            exception.Message.ShouldContain("~/b");
        }

        void SetupAsset(string filename, Mock<IAsset> asset)
        {
            asset.Setup(a => a.SourceFile.FullPath).Returns(filename);
            asset.Setup(a => a.Accept(It.IsAny<IBundleVisitor>()))
                 .Callback<IBundleVisitor>(v => v.Visit(asset.Object));
        }
    }
}