using System.Xml.Linq;
using Cassette.Manifests;
using Cassette.Utilities;

namespace Cassette.Scripts.Manifests
{
    class ScriptBundleManifestReader<T> : BundleManifestReader<T>
        where T : ScriptBundleManifest, new()
    {
        protected ScriptBundleManifestReader(XElement element) : base(element)
        {
        }

        protected override void InitializeBundleManifest(T manifest, XElement manifestElement)
        {
            manifest.Condition = manifestElement.AttributeValueOrNull("Condition");
        }
    }

    class ScriptBundleManifestReader : BundleManifestReader<ScriptBundleManifest>
    {
        public ScriptBundleManifestReader(XElement element) 
            : base(element)
        {
        }

        protected override void InitializeBundleManifest(ScriptBundleManifest manifest, XElement manifestElement)
        {
            base.InitializeBundleManifest(manifest, manifestElement);

            manifest.Condition = manifestElement.AttributeValueOrNull("Condition");
        }
    }
}