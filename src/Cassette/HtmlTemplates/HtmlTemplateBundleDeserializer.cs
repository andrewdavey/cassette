using System.Xml.Linq;

namespace Cassette.HtmlTemplates
{
    class HtmlTemplateBundleDeserializer : BundleDeserializer<HtmlTemplateBundle>
    {
        public HtmlTemplateBundleDeserializer(IUrlModifier urlModifier)
            : base(urlModifier)
        {
        }

        protected override HtmlTemplateBundle CreateBundle(XElement element)
        {
            return new HtmlTemplateBundle(GetPathAttribute())
            {
                Renderer = CreateHtmlRenderer<HtmlTemplateBundle>()
            };
        }
    }
}