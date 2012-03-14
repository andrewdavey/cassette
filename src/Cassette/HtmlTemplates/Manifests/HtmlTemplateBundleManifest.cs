using Cassette.Configuration;
using Cassette.Manifests;

namespace Cassette.HtmlTemplates.Manifests
{
    class HtmlTemplateBundleManifest : BundleManifest
    {
        protected override Bundle CreateBundleCore(CassetteSettings settings)
        {
            return new HtmlTemplateBundle(Path)
            {
                Renderer = new ConstantHtmlRenderer<HtmlTemplateBundle>(Html(), settings.UrlModifier)
            };
        }
    }
}