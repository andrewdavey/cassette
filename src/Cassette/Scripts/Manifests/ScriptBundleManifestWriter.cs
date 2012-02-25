using System.Xml.Linq;
using Cassette.Manifests;

namespace Cassette.Scripts.Manifests
{
    class ScriptBundleManifestWriter<T> : BundleManifestWriter<T>
        where T : ScriptBundleManifest
    {
        XElement element;

        protected ScriptBundleManifestWriter(XContainer container)
            : base(container)
        {
        }

        protected override XElement CreateElement()
        {
            element = base.CreateElement();
            AddConditionIfNotNull();

            return element;
        }

        void AddConditionIfNotNull()
        {
            if (Manifest.Condition != null)
            {
                element.Add(new XAttribute("Condition", Manifest.Condition));
            }
        }

    }

    class ScriptBundleManifestWriter : ScriptBundleManifestWriter<ScriptBundleManifest>
    {
        public ScriptBundleManifestWriter(XContainer container) 
            : base(container)
        {
        }
    }
}