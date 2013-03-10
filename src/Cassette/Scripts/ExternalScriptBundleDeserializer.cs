using System.Xml.Linq;
using Cassette.TinyIoC;
using Cassette.Utilities;

namespace Cassette.Scripts
{
    class ExternalScriptBundleDeserializer : ScriptBundleDeserializerBase<ExternalScriptBundle>
    {
        public ExternalScriptBundleDeserializer(TinyIoCContainer container)
            : base(container)
        {
        }

        protected override ExternalScriptBundle CreateBundle(XElement element)
        {
            var url = GetUrlAttribute(element);
            var path = GetPathAttribute();
            var fallbackCondition = element.AttributeValueOrNull("FallbackCondition");

            var externalScriptBundle = new ExternalScriptBundle(url, path, fallbackCondition);
            AssignScriptBundleProperties(externalScriptBundle, element);
            externalScriptBundle.FallbackRenderer = CreateHtmlRenderer<ScriptBundle>("FallbackRenderer");
            return externalScriptBundle;
        }

        string GetUrlAttribute(XElement manifestElement)
        {
            return manifestElement.AttributeValueOrThrow(
                "Url",
                () => new CassetteDeserializationException("ExternalScriptBundle manifest element is missing \"Url\" attribute.")
            );
        }
    }
}