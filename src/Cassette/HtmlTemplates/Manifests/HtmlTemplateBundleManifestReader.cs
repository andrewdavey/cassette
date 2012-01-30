using System.Xml.Linq;
using Cassette.Manifests;

namespace Cassette.HtmlTemplates.Manifests
{
    class HtmlTemplateBundleManifestReader : BundleManifestReader<HtmlTemplateBundleManifest>
    {
        public HtmlTemplateBundleManifestReader(XElement manifestElement) : base(manifestElement)
        {
        }
    }
}