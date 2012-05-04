using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    class ExternalStylesheetBundleDeserializer : StylesheetBundleDeserializerBase<ExternalStylesheetBundle>
    {
        public ExternalStylesheetBundleDeserializer(IUrlModifier urlModifier) 
            : base(urlModifier)
        {
        }

        protected override ExternalStylesheetBundle CreateBundle(XElement element)
        {
            var bundle = new ExternalStylesheetBundle(GetUrlAttribute(element), GetPathAttribute());
            AssignStylesheetBundleProperties(bundle);
            return bundle;
        }

        string GetUrlAttribute(XElement manifestElement)
        {
            return manifestElement.AttributeValueOrThrow(
                "Url",
                () => new CassetteDeserializationException("ExternalStylesheetBundle manifest element is missing \"Url\" attribute.")
            );
        }
    }
}