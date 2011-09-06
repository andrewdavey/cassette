using System.Web;
using Cassette.ModuleProcessing;

namespace Cassette.Scripts
{
    public class ScriptModule : Module
    {
        public ScriptModule(string directory)
            : base(directory)
        {
            ContentType = "text/javascript";
            Processor = new ScriptPipeline();
        }

        public IModuleProcessor<ScriptModule> Processor { get; set; }

        public IModuleHtmlRenderer<ScriptModule> Renderer { get; set; }

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
