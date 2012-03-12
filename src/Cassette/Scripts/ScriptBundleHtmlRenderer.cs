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
            var content = string.Format(
                HtmlConstants.ScriptHtml,
                url,
                bundle.HtmlAttributes.CombinedAttributes
            );

            var conditionalRenderer = new ConditionalRenderer();
            return conditionalRenderer.Render(bundle.Condition, html => html.Append(content));
        }
    }
}