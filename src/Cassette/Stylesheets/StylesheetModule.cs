using System.Collections.Generic;
using System.Linq;
using System.Web;
using Cassette.ModuleProcessing;
using System.Xml.Linq;

namespace Cassette.Stylesheets
{
    public class StylesheetModule : Module
    {
        public StylesheetModule(string directory)
            : base(directory)
        {
            ContentType = "text/css";
            Processor = new StylesheetPipeline();
        }

        protected static readonly string LinkHtml = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\"/>";
        protected static readonly string LinkHtmlWithMedia = "<link href=\"{0}\" type=\"text/css\" rel=\"stylesheet\" media=\"{1}\"/>";

        public string Media { get; set; }
        
        public IModuleProcessor<StylesheetModule> Processor { get; set; }

        public IModuleHtmlRenderer<StylesheetModule> Renderer { get; set; }

        public override void Process(ICassetteApplication application)
        {
            Processor.Process(this, application);
        }

        public override IHtmlString Render()
        {
            return Renderer.Render(this);
        }
    }
}