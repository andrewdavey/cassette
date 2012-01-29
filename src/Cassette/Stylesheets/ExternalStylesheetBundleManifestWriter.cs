using System.Xml.Linq;

namespace Cassette.Stylesheets
{
    class ExternalStylesheetBundleManifestWriter : StylesheetBundleManifestWriter<ExternalStylesheetBundleManifest>
    {
        public ExternalStylesheetBundleManifestWriter(XContainer container)
            : base(container)
        {
        }

        protected override XElement CreateElement(ExternalStylesheetBundleManifest manifest)
        {
            var element = base.CreateElement(manifest);
            element.Add(new XAttribute("Url", manifest.Url));
            return element;
        }
    }
}