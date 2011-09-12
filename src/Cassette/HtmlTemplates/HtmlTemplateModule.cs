using System.Web;
using Cassette.ModuleProcessing;

namespace Cassette.HtmlTemplates
{
    public class HtmlTemplateModule : Module
    {
        public HtmlTemplateModule(string directory)
            : base(directory)
        {
            ContentType = "text/html";
            Processor = new HtmlTemplatePipeline();
        }

        public IModuleProcessor<HtmlTemplateModule> Processor { get; set; }
        
        public IModuleHtmlRenderer<HtmlTemplateModule> Renderer { get; set; }

        public override void Process(ICassetteApplication application)
        {
            Processor.Process(this, application);
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            return Renderer.Render(this);
        }

        public string GetTemplateId(IAsset asset)
        {
            var path = asset.SourceFilename
                .Substring(Path.Length + 1)
                .Replace(System.IO.Path.DirectorySeparatorChar, '-')
                .Replace(System.IO.Path.AltDirectorySeparatorChar, '-');
            return System.IO.Path.GetFileNameWithoutExtension(path);
        }
    }
}