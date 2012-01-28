using System.Xml.Linq;

namespace Cassette.Stylesheets
{
    class ExternalStylesheetBundleManifest : StylesheetBundleManifest
    {
        public string Url { get; set; }

        protected override Bundle CreateBundleCore()
        {
            return new ExternalStylesheetBundle(Url, Path)
            {
                Media = Media
            };
        }

        public override void InitializeFromXElement(XElement element)
        {
            base.InitializeFromXElement(element);
            Url = element.AttributeOrThrow("Url", () => new InvalidBundleManifestException("ExternalStylesheetBundle manifest element is missing \"Url\" attribute."));
        }
    }
}