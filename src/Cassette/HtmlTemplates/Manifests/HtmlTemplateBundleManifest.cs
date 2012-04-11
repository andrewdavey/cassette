using Cassette.Manifests;

namespace Cassette.HtmlTemplates.Manifests
{
    class HtmlTemplateBundleManifest : BundleManifest
    {
        protected override Bundle CreateBundleCore(IUrlModifier urlModifier)
        {
            return new HtmlTemplateBundle(Path)
            {
                Renderer = new ConstantHtmlRenderer<HtmlTemplateBundle>(Html(), urlModifier)
            };
        }
    }
}