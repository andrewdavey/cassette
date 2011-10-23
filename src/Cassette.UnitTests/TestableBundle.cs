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

        public bool WasProcessed { get; set; }

        public bool WasDisposed { get; set; }

        public string RenderResult { get; set; }

        internal override IHtmlString Render()
        {
            return new HtmlString(RenderResult);
        }

        internal override void Process(ICassetteApplication application)
        {
            WasProcessed = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            WasDisposed = true;
        }
    }
}