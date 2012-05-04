using System.Xml.Linq;

namespace Cassette.Stylesheets
{
    class StylesheetBundleDeserializer : StylesheetBundleDeserializerBase<StylesheetBundle>
    {
        public StylesheetBundleDeserializer(IUrlModifier urlModifier) 
            : base(urlModifier)
        {
        }

        protected override StylesheetBundle CreateBundle(XElement element)
        {
            var stylesheetBundle = new StylesheetBundle(GetPathAttribute());
            AssignStylesheetBundleProperties(stylesheetBundle);
            return stylesheetBundle;
        }
    }
}