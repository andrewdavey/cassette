using System;
using System.Linq;
using System.Web;

namespace Cassette.Scripts
{
    public class DebugScriptModuleHtmlRenderer : IModuleHtmlRenderer<ScriptModule>
    {
        public DebugScriptModuleHtmlRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly IUrlGenerator urlGenerator;

        public IHtmlString Render(ScriptModule module)
        {
            var assetScripts = 
                from asset in module.Assets
                select string.Format(
                    HtmlConstants.ScriptHtml,
                    urlGenerator.CreateAssetUrl(asset)
                    );

            var html = string.Join(Environment.NewLine, assetScripts);
            return new HtmlString(html);
        }
    }
}