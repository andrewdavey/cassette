using System.Xml.Linq;
using Cassette.Manifests;

namespace Cassette.Scripts.Manifests
{
    class ScriptBundleManifestWriter : BundleManifestWriter<ScriptBundleManifest>
    {
        public ScriptBundleManifestWriter(XContainer container) : base(container)
        {
        }
    }
}