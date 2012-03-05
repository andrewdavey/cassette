
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
            
            html.AppendFormat(
                HtmlConstants.ScriptHtml,
                url,
                bundle.HtmlAttributes.CombinedAttributes
            );

            if (bundle.HasCondition)
            {
                return new ConditionalRenderer().RenderCondition(bundle.Condition, html.ToString());
            }
            else
            {
                return html.ToString();
            }

        }
    }
}
