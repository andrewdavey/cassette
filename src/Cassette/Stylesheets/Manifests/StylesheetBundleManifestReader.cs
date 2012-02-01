using System.Xml.Linq;
using Cassette.Manifests;
using Cassette.Utilities;

namespace Cassette.Stylesheets.Manifests
{
    class StylesheetBundleManifestReader<T> : BundleManifestReader<T>
        where T : StylesheetBundleManifest, new()
    {
        protected StylesheetBundleManifestReader(XElement element) : base(element)
        {
        }

        protected override void InitializeBundleManifest(T manifest, XElement manifestElement)
        {
            manifest.Condition = manifestElement.AttributeValueOrNull("Condition");
            manifest.Media = manifestElement.AttributeValueOrNull("Media");
        }
    }

    class StylesheetBundleManifestReader : StylesheetBundleManifestReader<StylesheetBundleManifest>
    {
        public StylesheetBundleManifestReader(XElement element)
            : base(element)
        {
        }
    }
}