using System.Xml.Linq;
using Cassette.Manifests;
using Cassette.Utilities;

namespace Cassette.Stylesheets.Manifests
{
    class ExternalStylesheetBundleManifestReader : StylesheetBundleManifestReader<ExternalStylesheetBundleManifest>
    {
        public ExternalStylesheetBundleManifestReader(XElement element)
            : base(element)
        {   
        }

        protected override void InitializeBundleManifest(ExternalStylesheetBundleManifest manifest, XElement manifestElement)
        {
            base.InitializeBundleManifest(manifest, manifestElement);
            manifest.Url = GetUrlAttribute(manifestElement);
        }

        string GetUrlAttribute(XElement manifestElement)
        {
            return manifestElement.AttributeValueOrThrow(
                "Url",
                () => new InvalidCassetteManifestException("ExternalStylesheetBundle manifest element is missing \"Url\" attribute.")
            );
        }
    }
}