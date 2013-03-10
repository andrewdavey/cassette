using System.Xml.Linq;
using Cassette.TinyIoC;

namespace Cassette.Stylesheets
{
    class StylesheetBundleDeserializer : StylesheetBundleDeserializerBase<StylesheetBundle>
    {
        public StylesheetBundleDeserializer(IUrlModifier urlModifier, TinyIoCContainer container) 
            : base(urlModifier, container)
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