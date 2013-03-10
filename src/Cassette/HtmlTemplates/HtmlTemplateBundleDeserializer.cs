using System.Xml.Linq;
using Cassette.TinyIoC;

namespace Cassette.HtmlTemplates
{
    class HtmlTemplateBundleDeserializer : BundleDeserializer<HtmlTemplateBundle>
    {
        public HtmlTemplateBundleDeserializer(TinyIoCContainer container)
            : base(container)
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