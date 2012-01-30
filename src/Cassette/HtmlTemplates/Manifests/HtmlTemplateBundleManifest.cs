using Cassette.Manifests;

namespace Cassette.HtmlTemplates.Manifests
{
    class HtmlTemplateBundleManifest : BundleManifest
    {
        protected override Bundle CreateBundleCore()
        {
            return new HtmlTemplateBundle(Path);
        }
    }
}