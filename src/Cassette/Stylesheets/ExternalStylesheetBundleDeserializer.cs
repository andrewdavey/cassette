using System.Xml.Linq;
using Cassette.TinyIoC;
using Cassette.Utilities;

namespace Cassette.Stylesheets
{
    class ExternalStylesheetBundleDeserializer : StylesheetBundleDeserializerBase<ExternalStylesheetBundle>
    {
        public ExternalStylesheetBundleDeserializer(TinyIoCContainer container) 
            : base(container)
        {
        }

        protected override ExternalStylesheetBundle CreateBundle(XElement element)
        {
            var bundle = new ExternalStylesheetBundle(GetUrlAttribute(element), GetPathAttribute());
            AssignStylesheetBundleProperties(bundle);
            bundle.FallbackRenderer = CreateHtmlRenderer<StylesheetBundle>("FallbackRenderer");
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