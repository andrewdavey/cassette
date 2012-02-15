
using System.Text;

namespace Cassette.Scripts
{
    class ScriptBundleHtmlRenderer : IBundleHtmlRenderer<ScriptBundle>
    {
        public ScriptBundleHtmlRenderer(IUrlGenerator urlGenerator)
        {
            this.urlGenerator = urlGenerator;
        }

        readonly IUrlGenerator urlGenerator;

        public string Render(ScriptBundle bundle)
        {
            var url = urlGenerator.CreateBundleUrl(bundle);
            var html = new StringBuilder();

            var hasCondition = !string.IsNullOrEmpty(bundle.Condition);
            if (hasCondition)
            {
                html.AppendFormat(HtmlConstants.ConditionalCommentStart, bundle.Condition);
                html.AppendLine();
            }

            html.AppendFormat(
                HtmlConstants.ScriptHtml,
                url,
                bundle.HtmlAttributes.CombinedAttributes
            );

            if (hasCondition)
            {
                html.AppendLine();
                html.Append(HtmlConstants.ConditionalCommentEnd);
            }

            return html.ToString();
        }
    }
}
