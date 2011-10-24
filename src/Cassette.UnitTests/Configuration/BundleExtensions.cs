using Should;
using Xunit;

namespace Cassette.Configuration
{
    public class BundleExtensions_Tests
    {
        [Fact]
        public void WhenAddFile_ThenAddFileBundleInitializerAddedToBundle()
        {
            var bundle = new TestableBundle("~");
            bundle.AddFile("asset.js");

            bundle.BundleInitializers[0].ShouldBeType<AddFileBundleInitializer>();
        }
    }
}