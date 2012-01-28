using Cassette.BundleProcessing;
using Cassette.Configuration;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateBundle : Bundle
    {
        public HtmlTemplateBundle(string applicationRelativePath)
            : base(applicationRelativePath)
        {
            ContentType = "text/html";
            Processor = new HtmlTemplatePipeline();
        }

        public IBundleProcessor<HtmlTemplateBundle> Processor { get; set; }
        
        public IBundleHtmlRenderer<HtmlTemplateBundle> Renderer { get; set; }

        internal override void Process(CassetteSettings settings)
        {
            Processor.Process(this, settings);
        }

        internal override string Render()
        {
            return Renderer.Render(this);
        }

        internal string GetTemplateId(IAsset asset)
        {
            var path = asset.SourceFile.FullPath
                .Substring(Path.Length + 1)
                .Replace(System.IO.Path.DirectorySeparatorChar, '-')
                .Replace(System.IO.Path.AltDirectorySeparatorChar, '-');
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }

        internal override BundleManifest CreateBundleManifest()
        {
            return new HtmlTemplateBundleManifestBuilder().BuildManifest(this);
        }
    }
}