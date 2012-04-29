using System.Xml.Linq;
using Cassette.IO;
using Cassette.Manifests;
using Cassette.Utilities;

namespace Cassette.Stylesheets.Manifests
{
    class ExternalStylesheetBundleDeserializer : StylesheetBundleDeserializerBase<ExternalStylesheetBundle>
    {
        public ExternalStylesheetBundleDeserializer(IDirectory directory, IUrlModifier urlModifier) 
            : base(directory, urlModifier)
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