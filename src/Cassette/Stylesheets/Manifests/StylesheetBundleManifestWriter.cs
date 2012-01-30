using System.Xml.Linq;

namespace Cassette.Stylesheets.Manifests
{
    class StylesheetBundleManifestWriter<T> : BundleManifestWriter<T>
        where T : StylesheetBundleManifest
    {
        protected StylesheetBundleManifestWriter(XContainer container)
            : base(container)
        {
        }

        protected override XElement CreateElement(T manifest)
        {
            var element = base.CreateElement(manifest);
            if (manifest.Media != null)
            {
                element.Add(new XAttribute("Media", manifest.Media));
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