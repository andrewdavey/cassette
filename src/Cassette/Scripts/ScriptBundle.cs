using Cassette.BundleProcessing;
using Cassette.Configuration;
using Cassette.Manifests;
using Cassette.Scripts.Manifests;
using System;

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

        /// <summary>
        /// The Internet Explorer specific condition used control if the script should be loaded using an HTML conditional comment.
        /// For example, <example>"lt IE 9"</example>.
        /// </summary>
        public string Condition { get; set; }

        internal bool HasCondition
        {
            get { return !String.IsNullOrEmpty(Condition); }
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

        internal override BundleManifest CreateBundleManifest(bool includeProcessedBundleContent)
        {
            var builder = new ScriptBundleManifestBuilder { IncludeContent = includeProcessedBundleContent};
            return builder.BuildManifest(this);
        }
    }
}