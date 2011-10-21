using System.Collections.Generic;
using System.Web;

namespace Cassette
{
    // Subclass so we can test the non-abstract implementation.
    public class TestableBundle : Bundle
    {
        public TestableBundle(string path) : base(path)
        {
        }

        public TestableBundle(string path, IEnumerable<IBundleInitializer> bundleInitializers)
            : base(path, bundleInitializers)
        {
        }

        public override IHtmlString Render()
        {
            throw new System.NotImplementedException();
        }
    }
}