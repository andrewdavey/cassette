namespace Cassette.HtmlTemplates
{
    public static class HtmlTemplateBundleExtensions
    {
        public static void AsJavaScript(this HtmlTemplateBundle bundle)
        {
            bundle.Pipeline.ReplaceWith<JavaScriptHtmlTemplatePipeline>();
        }
    }
}