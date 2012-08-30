using System.Xml.Linq;

namespace Cassette.Stylesheets
{
    class StylesheetBundleSerializer : StylesheetBundleSerializerBase<StylesheetBundle>
    {
        public StylesheetBundleSerializer(XContainer container) 
            : base(container)
        {
        }
    }
}