using System.Xml.Linq;
using Cassette.Manifests;

namespace Cassette.Stylesheets.Manifests
{
    class StylesheetBundleManifestWriter<T> : BundleManifestWriter<T>
        where T : StylesheetBundleManifest
    {
        protected StylesheetBundleManifestWriter(XContainer container)
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
            if (Manifest.Media != null)
            {
                element.Add(new XAttribute("Media", Manifest.Media));
            }
            return element;
        }
    }

    class StylesheetBundleManifestWriter : StylesheetBundleManifestWriter<StylesheetBundleManifest>
    {
        public StylesheetBundleManifestWriter(XContainer container) 
            : base(container)
        {
        }
    }
}