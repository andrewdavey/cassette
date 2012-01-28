using System.Xml.Linq;

namespace Cassette.Scripts
{
    class ExternalScriptBundleManifest : ScriptBundleManifest
    {
        public string Url { get; set; }
        public string FallbackCondition { get; set; }

        protected override Bundle CreateBundleCore()
        {
            return new ExternalScriptBundle(Url, Path, FallbackCondition);
        }

        public override void InitializeFromXElement(XElement manifestElement)
        {
            base.InitializeFromXElement(manifestElement);
            Url = manifestElement.AttributeOrThrow("Url", () => new InvalidBundleManifestException("ExternalScriptBundle manifest element is missing \"Url\" attribute."));
            FallbackCondition = manifestElement.AttributeValueOrNull("FallbackCondition");
        }
    }
}