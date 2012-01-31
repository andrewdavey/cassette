using Cassette.Configuration;
using Cassette.Manifests;

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

        internal override void Process(CassetteSettings settings)
        {
            WasProcessed = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            WasDisposed = true;
        }

        internal override BundleManifest CreateBundleManifest()
        {
            var builder = new BundleManifestBuilder<TestableBundle, TestableBundleManifest>();
            return builder.BuildManifest(this);
        }

        internal override BundleManifest CreateBundleManifestIncludingContent()
        {
            var builder = new BundleManifestBuilder<TestableBundle, TestableBundleManifest> { IncludeContent = true };
            return builder.BuildManifest(this);
        }

        class TestableBundleManifest : BundleManifest
        {
            protected override Bundle CreateBundleCore()
            {
                return new TestableBundle(Path) { Hash = Hash };
            }
        }
    }
}
