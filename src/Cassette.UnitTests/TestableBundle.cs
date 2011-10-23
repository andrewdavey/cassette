using System.Web;

namespace Cassette
{
    // Subclass so we can test the non-abstract implementation.
    public class TestableBundle : Bundle
    {
        public TestableBundle(string path) : base(path, true)
        {
        }

        public TestableBundle(string path, bool useDefaultBundleInitializer)
            : base(path, useDefaultBundleInitializer)
        {
        }

        internal override IHtmlString Render()
        {
            throw new System.NotImplementedException();
        }
    }
}