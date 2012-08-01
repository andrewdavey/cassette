using System;
using System.Collections.Generic;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleCollection_Tests
    {
        readonly BundleCollection bundles = new BundleCollection(new CassetteSettings(), Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());

        [Fact]
        public void GivenBundleAdded_WhenGetByAppRelativePath_ThenBundleReturned()
        {
            var bundle = new TestableBundle("~/test");
            bundles.Add(bundle);

            var actualBundle = bundles.Get("~/test");

            actualBundle.ShouldBeSameAs(bundle);
        }

        [Fact]
        public void GivenBundleAdded_WhenGetByPartialPath_ThenBundleReturned()
        {
            var bundle = new TestableBundle("~/test");
            bundles.Add(bundle);

            var actualBundle = bundles.Get("test");

            actualBundle.ShouldBeSameAs(bundle);
        }

        [Fact]
        public void GivenNoBundles_WhenGetAnyPath_ThenExceptionThrown()
        {
            var exception = Assert.Throws<ArgumentException>(delegate
            {
                bundles.Get("~/any");
            });
            exception.Message.ShouldContain("~/any");
        }

        [Fact]
        public void GivenTwoBundlesWithSamePathButDifferentTypes_WhenGetPathWithType_ThenBundleMatchinTheTypeIsReturned()
        {
            var scriptBundle = new ScriptBundle("~/test");
            bundles.Add(scriptBundle);
            bundles.Add(new StylesheetBundle("~/test"));

            var bundle = bundles.Get<ScriptBundle>("~/test");

            bundle.ShouldBeSameAs(scriptBundle);
        }

        [Fact]
        public void GivenBundleAdded_WhenAddAnotherWithSamePath_ThenExceptionIsThrown()
        {
            var bundle = new TestableBundle("~/test");
            bundles.Add(bundle);

            Assert.Throws<ArgumentException>(
                () => bundles.Add(new TestableBundle("~/test"))
            );
        }

        [Fact]
        public void GivenBundlesAdded_WhenEnumerated_ThenBundlesReturned()
        {
            var bundle1 = new TestableBundle("~/test1");
            var bundle2 = new TestableBundle("~/test2");
            bundles.Add(bundle1);
            bundles.Add(bundle2);

            var set = new HashSet<Bundle>(bundles);

            set.SetEquals(new[] { bundle1, bundle2 }).ShouldBeTrue();
        }

        [Fact]
        public void GivenBundleThatReturnsTrueForContainsPathWithOtherPath_WhenGetByOtherPath_ThenBundleReturned()
        {
            var bundle = new Mock<TestableBundle>("~/path");
            bundle.Setup(b => b.ContainsPath("~/OTHER")).Returns(true);

            bundles.Add(bundle.Object);

            bundles["OTHER"].ShouldBeSameAs(bundle.Object);
        }

        [Fact]
        public void GivenWriteLockIsAlreadyHeld_GetWriteLockAllowsARecursiveLock() // refs #267
        {
            using(bundles.GetWriteLock())
            {
                using (bundles.GetWriteLock())
                {
                }
            }
        }
    }
}
