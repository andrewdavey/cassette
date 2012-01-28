using System.Xml.Linq;
namespace Cassette.Stylesheets
{
    class StylesheetBundleManifest : BundleManifest
    {
        public string Media { get; set; }

        protected override Bundle CreateBundleCore()
        {
            return new StylesheetBundle(Path)
            {
                Media = Media
            };
        }

        public override void InitializeFromXElement(XElement element)
        {
            base.InitializeFromXElement(element);
            Media = element.AttributeValueOrNull("Media");
        }
    }
}