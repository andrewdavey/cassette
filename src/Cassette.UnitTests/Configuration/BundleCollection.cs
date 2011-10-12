using System;
using System.Collections.Generic;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class BundleCollection_Tests
    {
        [Fact]
        public void GivenBundleAdded_WhenGetByAppRelativePath_ThenBundleReturned()
        {
            var bundles = new BundleCollection();
            var bundle = new Bundle("~/test");
            bundles.Add(bundle);

            var actualBundle = bundles.Get("~/test");

            actualBundle.ShouldBeSameAs(bundle);
        }

        [Fact]
        public void GivenBundleAdded_WhenGetByPartialPath_ThenBundleReturned()
        {
            var bundles = new BundleCollection();
            var bundle = new Bundle("~/test");
            bundles.Add(bundle);

            var actualBundle = bundles.Get("test");

            actualBundle.ShouldBeSameAs(bundle);
        }

        [Fact]
        public void GivenNoBundles_WhenGetAnyPath_ThenExceptionThrown()
        {
            var bundles = new BundleCollection();

            var exception = Assert.Throws<ArgumentException>(delegate
            {
                bundles.Get("~/any");
            });
            exception.Message.ShouldContain("~/any");
        }

        [Fact]
        public void GivenTwoBundlesWithSamePathButDifferentTypes_WhenGetPathWithType_ThenBundleMatchinTheTypeIsReturned()
        {
            var bundles = new BundleCollection();
            var scriptBundle = new ScriptBundle("~/test");
            bundles.Add(scriptBundle);
            bundles.Add(new StylesheetBundle("~/test"));

            var bundle = bundles.Get<ScriptBundle>("~/test");

            bundle.ShouldBeSameAs(scriptBundle);
        }

        [Fact]
        public void GivenBundleAdded_WhenAddAnotherWithSamePath_ThenExceptionIsThrown()
        {
            var bundles = new BundleCollection();
            var bundle = new Bundle("~/test");
            bundles.Add(bundle);

            Assert.Throws<ArgumentException>(
                () => bundles.Add(new Bundle("~/test"))
            );
        }

        [Fact]
        public void GivenBundlesAdded_WhenEnumerated_ThenBundlesReturned()
        {
            var bundle1 = new Bundle("~/test1");
            var bundle2 = new Bundle("~/test2");
            var bundles = new BundleCollection
            {
                bundle1,
                bundle2
            };

            var set = new HashSet<Bundle>(bundles);

            set.SetEquals(new[] { bundle1, bundle2 }).ShouldBeTrue();
        }
    }
}
