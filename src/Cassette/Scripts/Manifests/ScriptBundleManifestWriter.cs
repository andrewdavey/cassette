using System.Xml.Linq;
using Cassette.Manifests;

namespace Cassette.Scripts.Manifests
{
    class ScriptBundleManifestWriter<T> : BundleManifestWriter<T>
        where T : ScriptBundleManifest
    {
        protected ScriptBundleManifestWriter(XContainer container)
            : base(container)
        {
        }

        protected override XElement CreateElement()
        {
            var element = base.CreateElement();
            if (Manifest.Condition != null)
            {
                element.Add(new XAttribute("Condition", Manifest.Condition));
            }

            return element;
        }
    }

    class ScriptBundleManifestWriter : BundleManifestWriter<ScriptBundleManifest>
    {
        public ScriptBundleManifestWriter(XContainer container) 
            : base(container)
        {
        }

        protected override XElement CreateElement()
        {
            var element = base.CreateElement();

            if (Manifest.Condition != null)
            {
                element.Add(new XAttribute("Condition", Manifest.Condition));
            }

            return element;
        }
    }
}