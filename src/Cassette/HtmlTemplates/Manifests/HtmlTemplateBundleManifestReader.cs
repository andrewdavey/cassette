using System.Xml.Linq;

namespace Cassette.HtmlTemplates.Manifests
{
    class HtmlTemplateBundleManifestReader : BundleManifestReader<HtmlTemplateBundleManifest>
    {
        public HtmlTemplateBundleManifestReader(XElement manifestElement) : base(manifestElement)
        {
        }
    }
}