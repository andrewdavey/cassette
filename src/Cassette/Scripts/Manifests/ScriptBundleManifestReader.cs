using System.Xml.Linq;

namespace Cassette.Scripts.Manifests
{
    class ScriptBundleManifestReader : BundleManifestReader<ScriptBundleManifest>
    {
        public ScriptBundleManifestReader(XElement element) : base(element)
        {
        }
    }
}