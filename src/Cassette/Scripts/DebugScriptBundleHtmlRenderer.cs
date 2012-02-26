using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            var html = new StringBuilder();

            var hasCondition = !string.IsNullOrEmpty(bundle.Condition);
            if (hasCondition)
            {
                html.AppendFormat(HtmlConstants.ConditionalCommentStart, bundle.Condition);
                html.AppendLine();
            }

            html.Append(string.Join(
                Environment.NewLine,
                assetUrls.Select(createLink)
            ));

            if (hasCondition)
            {
                html.AppendLine();
                html.Append(HtmlConstants.ConditionalCommentEnd);
            }

            return html.ToString();
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
