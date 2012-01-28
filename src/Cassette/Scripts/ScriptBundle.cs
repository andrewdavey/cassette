using Cassette.BundleProcessing;
using Cassette.Configuration;

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

        internal override void Process(CassetteSettings settings)
        {
            Processor.Process(this, settings);
        }

        internal override string Render()
        {
            return Renderer.Render(this);
        }

        internal override BundleManifest CreateBundleManifest()
        {
            return new ScriptBundleManifestBuilder().BuildManifest(this);
        }
    }
}

