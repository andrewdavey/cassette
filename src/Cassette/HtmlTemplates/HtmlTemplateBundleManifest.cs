namespace Cassette.HtmlTemplates
{
    class HtmlTemplateBundleManifest : BundleManifest
    {
        protected override Bundle CreateBundleCore()
        {
            return new HtmlTemplateBundle(Path);
        }
    }
}