using System;
using System.Linq;

namespace Cassette.HtmlTemplates
{
    class InlineHtmlTemplateBundleRenderer : IBundleHtmlRenderer<HtmlTemplateBundle>
    {
        public string Render(HtmlTemplateBundle bundle)
        {
            return string.Join(
                Environment.NewLine,
                bundle.Assets.Select(asset => asset.GetTransformedContent()).ToArray()
            );
        }
    }
}