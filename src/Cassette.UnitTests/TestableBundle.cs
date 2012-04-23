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

        protected override void ProcessCore(CassetteSettings settings)
        {
            WasProcessed = true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            WasDisposed = true;
        }

        internal override BundleManifest CreateBundleManifest(bool includeProcessedBundleContent)
        {
            var builder = new BundleManifestBuilder<TestableBundle, TestableBundleManifest> { IncludeContent = includeProcessedBundleContent };
            return builder.BuildManifest(this);
        }

        protected override string UrlBundleTypeArgument
        {
            get { return "testablebundle"; }
        }

        class TestableBundleManifest : BundleManifest
        {
            protected override Bundle CreateBundleCore(IUrlModifier urlModifier)
            {
                return new TestableBundle(Path) { Hash = Hash };
            }
        }
    }
}