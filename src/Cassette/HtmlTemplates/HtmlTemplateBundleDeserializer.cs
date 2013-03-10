using System.Xml.Linq;
using Cassette.TinyIoC;

namespace Cassette.HtmlTemplates
{
    class HtmlTemplateBundleDeserializer : BundleDeserializer<HtmlTemplateBundle>
    {
        public HtmlTemplateBundleDeserializer(IUrlModifier urlModifier, TinyIoCContainer container)
            : base(urlModifier, container)
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