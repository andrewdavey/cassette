using System.Xml.Linq;

namespace Cassette.Stylesheets.Manifests
{
    class ExternalStylesheetBundleManifestWriter : StylesheetBundleManifestWriter<ExternalStylesheetBundleManifest>
    {
        public ExternalStylesheetBundleManifestWriter(XContainer container)
            : base(container)
        {
        }

        protected override XElement CreateElement()
        {
            var element = base.CreateElement();
            element.Add(new XAttribute("Url", Manifest.Url));
            return element;
        }
    }
}