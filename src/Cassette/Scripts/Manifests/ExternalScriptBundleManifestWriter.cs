using System.Xml.Linq;

namespace Cassette.Scripts.Manifests
{
    class ExternalScriptBundleManifestWriter : ScriptBundleManifestWriter<ExternalScriptBundleManifest>
    {
        XElement element;

        public ExternalScriptBundleManifestWriter(XContainer container) : base(container)
        {
        }

        protected override XElement CreateElement()
        {
            element = base.CreateElement();

            AddUrlAttribute();
            AddFallbackConditionIfNotNull();

            return element;
        }

        void AddUrlAttribute()
        {
            element.Add(new XAttribute("Url", Manifest.Url));
        }

        void AddFallbackConditionIfNotNull()
        {
            if (Manifest.FallbackCondition != null)
            {
                element.Add(new XAttribute("FallbackCondition", Manifest.FallbackCondition));
            }
        }
    }
}