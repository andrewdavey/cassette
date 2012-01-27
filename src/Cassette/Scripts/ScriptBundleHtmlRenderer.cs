using Cassette.Utilities;

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
            return string.Format(
                HtmlConstants.ScriptHtml, 
                urlGenerator.CreateBundleUrl(bundle),
                bundle.HtmlAttributes.CombinedAttributes
            );
        }
    }
}
