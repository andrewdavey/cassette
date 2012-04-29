using System.Xml.Linq;
using Cassette.IO;

namespace Cassette.Stylesheets.Manifests
{
    class StylesheetBundleDeserializer : StylesheetBundleDeserializerBase<StylesheetBundle>
    {
        public StylesheetBundleDeserializer(IDirectory directory, IUrlModifier urlModifier) 
            : base(directory, urlModifier)
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