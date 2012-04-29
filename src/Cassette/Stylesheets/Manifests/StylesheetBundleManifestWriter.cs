using System.Xml.Linq;

namespace Cassette.Stylesheets.Manifests
{
    class StylesheetBundleSerializer : StylesheetBundleSerializerBase<StylesheetBundle>
    {
        public StylesheetBundleSerializer(XContainer container) 
            : base(container)
        {
        }
    }
}