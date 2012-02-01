using System;
using System.Linq;

namespace Cassette.Scripts
{
    class DebugScriptBundleHtmlRenderer : IBundleHtmlRenderer<ScriptBundle>
    {
        public DebugScriptBundleHtmlRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly IUrlGenerator urlGenerator;

        public string Render(ScriptBundle bundle)
        {
            var assetScripts = 
                from asset in bundle.Assets
                let url = urlGenerator.CreateAssetUrl(asset)
                select string.Format(
                    HtmlConstants.ScriptHtml, 
                    url, 
                    bundle.HtmlAttributes.CombinedAttributes
                );

            return string.Join(Environment.NewLine, assetScripts);
        }
    }
}
