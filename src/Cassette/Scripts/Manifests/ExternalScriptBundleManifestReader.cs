using System.Xml.Linq;
using Cassette.Manifests;
using Cassette.Utilities;

namespace Cassette.Scripts.Manifests
{
    class ExternalScriptBundleManifestReader : BundleManifestReader<ExternalScriptBundleManifest>
    {
        public ExternalScriptBundleManifestReader(XElement element)
            : base(element)
        {   
        }

        protected override void InitializeBundleManifest(ExternalScriptBundleManifest manifest, XElement manifestElement)
        {
            base.InitializeBundleManifest(manifest, manifestElement);
            manifest.Url = GetUrlAttribute(manifestElement);
            manifest.Condition = manifestElement.AttributeValueOrNull("Condition");
            manifest.FallbackCondition = manifestElement.AttributeValueOrNull("FallbackCondition");
        }

        string GetUrlAttribute(XElement manifestElement)
        {
            return manifestElement.AttributeValueOrThrow(
                "Url",
                () => new InvalidCassetteManifestException("ExternalScriptBundle manifest element is missing \"Url\" attribute.")
            );
        }
    }
}