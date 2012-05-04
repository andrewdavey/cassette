using System.Xml.Linq;
using Cassette.IO;

namespace Cassette.HtmlTemplates
{
    class HtmlTemplateBundleDeserializer : BundleDeserializer<HtmlTemplateBundle>
    {
        public HtmlTemplateBundleDeserializer(IDirectory directory, IUrlModifier urlModifier)
            : base(directory, urlModifier)
        {
        }

        protected override HtmlTemplateBundle CreateBundle(XElement element)
        {
            return new HtmlTemplateBundle(GetPathAttribute())
            {
                Renderer = CreateHtmlRenderer<HtmlTemplateBundle>()
            };
        }

        protected override string ContentFileExtension
        {
            get { return ".htm"; }
        }
    }
}