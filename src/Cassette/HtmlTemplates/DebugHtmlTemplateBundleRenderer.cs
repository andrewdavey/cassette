using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cassette.HtmlTemplates
{
    internal class DebugHtmlTemplateBundleRenderer : IBundleHtmlRenderer<HtmlTemplateBundle>
    {
        public DebugHtmlTemplateBundleRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly IUrlGenerator urlGenerator;

        public string Render(HtmlTemplateBundle bundle)
        {
            var assetUrls = GetAssetUrls(bundle);
            var createLink = GetCreateScriptFunc(bundle);
            return string.Join(
                Environment.NewLine,
                assetUrls.Select(createLink).ToArray()
                );
        }

        IEnumerable<string> GetAssetUrls(HtmlTemplateBundle bundle)
        {
            return bundle.Assets.Select(urlGenerator.CreateAssetUrl);
        }

        Func<string, string> GetCreateScriptFunc(HtmlTemplateBundle bundle)
        {
            return url => string.Format(
                HtmlConstants.RemoteHtml,
                url,
                bundle.HtmlAttributes.CombinedAttributes
            );
        }
    }
}
