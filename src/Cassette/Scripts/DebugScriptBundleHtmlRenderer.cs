using System;
using System.Linq;
using Cassette.Utilities;

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
                    bundle.HtmlAttributesDictionary == null ? string.Empty : bundle.HtmlAttributesDictionary.HtmlAttributesString());

            return string.Join(Environment.NewLine, assetScripts);
        }
    }
}
