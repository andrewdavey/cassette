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
    }
}