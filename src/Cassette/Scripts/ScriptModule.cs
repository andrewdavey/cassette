using System;
using System.Linq;
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

        protected static readonly string ScriptHtml = "<script src=\"{0}\" type=\"text/javascript\"></script>";

        public IModuleProcessor<ScriptModule> Processor { get; set; }

        public override void Process(ICassetteApplication application)
        {
            Processor.Process(this, application);
        }

        public override IHtmlString Render(ICassetteApplication application)
        {
            if (application.IsOutputOptimized)
            {
                // TODO: Extract rendering into pluggable class to allow customization?
                // Much like the Processor property allows.
                var url = application.UrlGenerator.CreateModuleUrl(this);
                return new HtmlString(string.Format(ScriptHtml, url));
            }
            else
            {
                var scripts = string.Join(Environment.NewLine, 
                    from asset in Assets
                    let url = IsCompiledAsset(asset)
                        ? application.UrlGenerator.CreateAssetCompileUrl(this, asset)
                        : application.UrlGenerator.CreateAssetUrl(this, asset)
                    select string.Format(ScriptHtml, url)
                );
                return new HtmlString(scripts);
            }
        }
    }
}
