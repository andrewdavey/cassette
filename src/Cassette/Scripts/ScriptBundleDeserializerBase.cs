using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette.Scripts
{
    abstract class ScriptBundleDeserializerBase<T> : BundleDeserializer<T>
        where T : ScriptBundle
    {
        protected ScriptBundleDeserializerBase(IUrlModifier urlModifier, IApplicationRootPrepender applicationRootPrepender)
            : base(urlModifier, applicationRootPrepender)
        {
        }

        protected void AssignScriptBundleProperties(T bundle, XElement element)
        {
            bundle.Condition = element.AttributeValueOrNull("Condition");
            bundle.Renderer = CreateHtmlRenderer<ScriptBundle>();
        }
    }
}