using Cassette.BundleProcessing;
using Cassette.HtmlTemplates.Manifests;
using Cassette.Manifests;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundle : Bundle
    {
        public HtmlTemplateBundle(string applicationRelativePath)
            : base(applicationRelativePath)
        {
            ContentType = "text/html";
        }

        public IBundleProcessor<HtmlTemplateBundle> Processor { get; set; }
        
        public IBundleHtmlRenderer<HtmlTemplateBundle> Renderer { get; set; }

        protected override void ProcessCore(CassetteSettings settings)
        {
            Processor.Process(this);
        }

        internal override string Render()
        {
            return Renderer.Render(this);
        }

        internal string GetTemplateId(IAsset asset)
        {
            var path = asset.Path
                .Substring(Path.Length + 1)
                .Replace(System.IO.Path.DirectorySeparatorChar, '-')
                .Replace(System.IO.Path.AltDirectorySeparatorChar, '-');
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }

        internal override BundleManifest CreateBundleManifest(bool includeProcessedBundleContent)
        {
            var builder = new HtmlTemplateBundleManifestBuilder { IncludeContent = includeProcessedBundleContent };
            return builder.BuildManifest(this);
        }

        protected override string UrlBundleTypeArgument
        {
            get { return "htmltemplate"; }
        }
    }
}