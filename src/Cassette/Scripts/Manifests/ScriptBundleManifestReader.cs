using System.Xml.Linq;
using Cassette.Manifests;

namespace Cassette.Scripts.Manifests
{
    class ScriptBundleManifestReader : BundleManifestReader<ScriptBundleManifest>
    {
        public ScriptBundleManifestReader(XElement element) : base(element)
        {
        }
    }
}