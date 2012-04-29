using System.Xml.Linq;
using Cassette.IO;
using Cassette.Manifests;

namespace Cassette.HtmlTemplates.Manifests
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
    }
}