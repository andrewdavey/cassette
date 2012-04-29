namespace Cassette
{
    // Subclass so we can test the non-abstract implementation.
    public class TestableBundle : Bundle
    {
        public TestableBundle(string path) : base(path)
        {
        }

        public bool WasProcessed { get; set; }

        public bool WasDisposed { get; set; }

        public string RenderResult { get; set; }

        internal override string Render()
        {
            return RenderResult;
        }

        protected override void ProcessCore(CassetteSettings settings)
        {
            WasProcessed = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            WasDisposed = true;
        }

        protected override string UrlBundleTypeArgument
        {
            get { return "testable"; }
        }
    }
}