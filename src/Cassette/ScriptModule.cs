using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Cassette
{
    public class ScriptModule : Module
    {
        public ScriptModule(string directory, IFileSystem fileSystem)
            : base(directory)
        {
            ContentType = "text/javascript";
        }

        static readonly string scriptHtml = "<script src=\"{0}\" type=\"text/javascript\"></script>";

        public override IHtmlString Render(ICassetteApplication application)
        {
            if (application.IsOutputOptimized)
            {
                var url = application.CreateModuleUrl(this);
                return new HtmlString(string.Format(scriptHtml, url));
            }
            else
            {
                var scripts = string.Join(Environment.NewLine, 
                    from asset in Assets
                    let url = application.CreateAssetUrl(this, asset)
                    select string.Format(scriptHtml, url)
                );
                return new HtmlString(scripts);
            }
        }
    }
}
