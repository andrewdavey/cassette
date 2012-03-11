using System;
using System.Collections.Generic;
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
            var assetUrls = GetAssetUrls(bundle);
            var createLink = GetCreateScriptFunc(bundle);
            var content = string.Join(
                Environment.NewLine,
                assetUrls.Select(createLink)
            );

            var conditionalRenderer = new ConditionalRenderer();
            return conditionalRenderer.Render(
                bundle.Condition,
                html => html.Append(content)
            );
        }

        IEnumerable<string> GetAssetUrls(ScriptBundle bundle)
        {
            return bundle.Assets.Select(urlGenerator.CreateAssetUrl);
        }

        Func<string, string> GetCreateScriptFunc(ScriptBundle bundle)
        {
            return url => string.Format(
                HtmlConstants.ScriptHtml, 
                url, 
                bundle.HtmlAttributes.CombinedAttributes
            );
        }
    }
}
