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
                let url = asset.HasTransformers
                    ? urlGenerator.CreateAssetCompileUrl(module, asset)
                    : urlGenerator.CreateAssetUrl(asset)
                select string.Format(HtmlConstants.ScriptHtml, url);

            return new HtmlString(
                string.Join(Environment.NewLine, assetScripts)
            );
        }
    }
}