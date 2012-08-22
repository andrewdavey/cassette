using System.Xml.Linq;

namespace Cassette.Stylesheets
{
    class StylesheetBundleDeserializer : StylesheetBundleDeserializerBase<StylesheetBundle>
    {
        public StylesheetBundleDeserializer(IUrlModifier urlModifier, IApplicationRootPrepender applicationRootPrepender)
            : base(urlModifier, applicationRootPrepender)
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