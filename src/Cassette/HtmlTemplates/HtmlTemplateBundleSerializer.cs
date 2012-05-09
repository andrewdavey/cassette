using System.Xml.Linq;

namespace Cassette.HtmlTemplates
{
    class HtmlTemplateBundleSerializer : BundleSerializer<HtmlTemplateBundle>
    {
        public HtmlTemplateBundleSerializer(XContainer container) : base(container)
        {
        }
    }
}