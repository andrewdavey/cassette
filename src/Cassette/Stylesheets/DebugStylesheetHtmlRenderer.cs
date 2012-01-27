using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    class DebugStylesheetHtmlRenderer : IBundleHtmlRenderer<StylesheetBundle>
    {
        public DebugStylesheetHtmlRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly IUrlGenerator urlGenerator;

        public string Render(StylesheetBundle bundle)
        {
            var assetUrls = GetAssetUrls(bundle);
            var createLink = GetCreateLinkFunc(bundle);
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

        IEnumerable<string> GetAssetUrls(StylesheetBundle bundle)
        {
            return bundle.Assets.Select(urlGenerator.CreateAssetUrl);
        }

        Func<string, string> GetCreateLinkFunc(StylesheetBundle bundle)
        {
            if (string.IsNullOrEmpty(bundle.Media))
            {
                return url => string.Format(
                    HtmlConstants.LinkHtml,
                    url,
                    bundle.HtmlAttributes.CombinedAttributes
                );
            }
            else
            {
                return url => string.Format(
                    HtmlConstants.LinkWithMediaHtml,
                    url,
                    bundle.Media,
                    bundle.HtmlAttributes.CombinedAttributes
                );
            }
        }
    }
}
