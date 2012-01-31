using Cassette.BundleProcessing;
using Cassette.Configuration;
using Cassette.Manifests;
using Cassette.Scripts.Manifests;

namespace Cassette.Scripts
{
    public class ScriptBundle : Bundle
    {
        public ScriptBundle(string applicationRelativePath)
            : base(applicationRelativePath)
        {
            ContentType = "text/javascript";
            Processor = new ScriptPipeline();
        }

        protected ScriptBundle()
        {
        }

        public IBundleProcessor<ScriptBundle> Processor { get; set; }

        public IBundleHtmlRenderer<ScriptBundle> Renderer { get; set; }

        protected override void ProcessCore(CassetteSettings settings)
        {
            Processor.Process(this, settings);
        }

        internal override string Render()
        {
            return Renderer.Render(this);
        }

        internal override BundleManifest CreateBundleManifest()
        {
            var builder = new ScriptBundleManifestBuilder();
            return builder.BuildManifest(this);
        }

        internal override BundleManifest CreateBundleManifestIncludingContent()
        {
            var builder = new ScriptBundleManifestBuilder { IncludeContent = true };
            return builder.BuildManifest(this);
        }
    }
}