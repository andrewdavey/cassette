using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleCollectionEquality_Tests
    {
        [Fact]
        public void EmptyBundleCollectionsAreEqual()
        {
            var collection1 = EmptyBundleCollection();
            var collection2 = EmptyBundleCollection();
            collection1.Equals(collection2).ShouldBeTrue();
        }

        [Fact]
        public void BundleCollectionsWithBundlesHavingDifferentPathsAreNotEqual()
        {
            var collection1 = EmptyBundleCollection();
            collection1.Add(new TestableBundle("~/bundle1"));
            var collection2 = EmptyBundleCollection();
            collection2.Add(new TestableBundle("~/bundle2"));
            collection1.Equals(collection2).ShouldBeFalse();
        }

        [Fact]
        public void BundleCollectionsWithBundlesOfDifferentTypesAreNotEqual()
        {
            var collection1 = EmptyBundleCollection();
            collection1.Add(new ScriptBundle("~/bundle"));
            var collection2 = EmptyBundleCollection();
            collection2.Add(new StylesheetBundle("~/bundle"));
            collection1.Equals(collection2).ShouldBeFalse();
        }

        [Fact]
        public void BundleCollectionsAreSortedByTypeBeforeEqualityComparison()
        {
            var collection1 = EmptyBundleCollection();
            collection1.Add(new ScriptBundle("~/bundle"));
            collection1.Add(new StylesheetBundle("~/bundle"));
            var collection2 = EmptyBundleCollection();
            collection2.Add(new StylesheetBundle("~/bundle"));
            collection2.Add(new ScriptBundle("~/bundle"));
            collection1.Equals(collection2).ShouldBeTrue();
        }

        [Fact]
        public void BundleCollectionsWithSameBundlesInDifferentOrdersAreEqual()
        {
            var collection1 = EmptyBundleCollection();
            collection1.Add(new TestableBundle("~/bundle1"));
            collection1.Add(new TestableBundle("~/bundle2"));
            var collection2 = EmptyBundleCollection();
            collection2.Add(new TestableBundle("~/bundle2"));
            collection2.Add(new TestableBundle("~/bundle1"));
            collection1.Equals(collection2).ShouldBeTrue();
        }

        BundleCollection EmptyBundleCollection()
        {
            return new BundleCollection(new CassetteSettings(), Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
        }
    }
}