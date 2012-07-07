using System.Collections.Generic;
using System.Linq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleCollection_ChangedEvent_Tests : BundleCollectionTestsBase
    {
        [Fact]
        public void WhenWriteLockReleased_ThenChangedEventRaised()
        {
            var changeEventRaised = false;
            bundles.Changed += delegate
            {
                changeEventRaised = true;
            };
            
            using (bundles.GetWriteLock())
            {
                bundles.Add(new TestableBundle("~"));
            }

            changeEventRaised.ShouldBeTrue();
        }

        [Fact]
        public void ChangeEventArgsContainsReadOnlyCopyOfBundlesCollection()
        {
            IEnumerable<Bundle> readOnlyBundleCollection = null;
            bundles.Changed += (sender, args) =>
            {
                readOnlyBundleCollection = args.Bundles;
            };

            var bundle = new TestableBundle("~");

            using (bundles.GetWriteLock())
            {
                bundles.Add(bundle);
            }

            readOnlyBundleCollection.First().ShouldBeSameAs(bundle);
        }
    }
}