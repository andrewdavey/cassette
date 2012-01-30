using System.Xml.Linq;

namespace Cassette.Scripts.Manifests
{
    class ExternalScriptBundleManifestWriter : BundleManifestWriter<ExternalScriptBundleManifest>
    {
        XElement element;
        ExternalScriptBundleManifest manifest;

        public ExternalScriptBundleManifestWriter(XContainer container) : base(container)
        {
        }

        protected override XElement CreateElement(ExternalScriptBundleManifest manifest)
        {
            element = base.CreateElement(manifest);
            this.manifest = manifest;

            AddUrlAttribute();
            AddFallbackConditionIfNotNull();

            return element;
        }

        void AddUrlAttribute()
        {
            element.Add(new XAttribute("Url", manifest.Url));
        }

        void AddFallbackConditionIfNotNull()
        {
            if (manifest.FallbackCondition != null)
            {
                element.Add(new XAttribute("FallbackCondition", manifest.FallbackCondition));
            }
        }
    }
}