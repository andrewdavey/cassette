using System.Xml.Linq;
using Cassette.Manifests;

namespace Cassette.HtmlTemplates.Manifests
{
    class HtmlTemplateBundleManifestWriter : BundleManifestWriter<HtmlTemplateBundleManifest>
    {
        public HtmlTemplateBundleManifestWriter(XContainer container) : base(container)
        {
        }
    }
}