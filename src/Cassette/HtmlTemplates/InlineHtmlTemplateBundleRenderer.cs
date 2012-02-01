using System;
using System.Linq;
using Cassette.Utilities;

namespace Cassette.HtmlTemplates
{
    class InlineHtmlTemplateBundleRenderer : IBundleHtmlRenderer<HtmlTemplateBundle>
    {
        public string Render(HtmlTemplateBundle bundle)
        {
            return string.Join(
                Environment.NewLine,
                bundle.Assets.Select(asset => asset.OpenStream().ReadToEnd())
            );
        }
    }
}