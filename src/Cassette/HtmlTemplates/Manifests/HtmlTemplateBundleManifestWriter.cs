using System.Xml.Linq;
using Cassette.Manifests;

namespace Cassette.HtmlTemplates.Manifests
{
    class HtmlTemplateBundleSerializer : BundleSerializer<HtmlTemplateBundle>
    {
        public HtmlTemplateBundleSerializer(XContainer container) : base(container)
        {
        }
    }
}