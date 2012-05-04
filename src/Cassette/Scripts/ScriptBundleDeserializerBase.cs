using System.Xml.Linq;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Scripts
{
    abstract class ScriptBundleDeserializerBase<T> : BundleDeserializer<T>
        where T : ScriptBundle
    {
        protected ScriptBundleDeserializerBase(IDirectory directory, IUrlModifier urlModifier)
            : base(directory, urlModifier)
        {
        }

        protected void AssignScriptBundleProperties(T bundle, XElement element)
        {
            bundle.Condition = element.AttributeValueOrNull("Condition");
            bundle.Renderer = CreateHtmlRenderer<ScriptBundle>();
        }

        protected override string ContentFileExtension
        {
            get { return ".js"; }
        }
    }
}