using System.Xml.Linq;
using Cassette.TinyIoC;

namespace Cassette.Scripts
{
    class ScriptBundleDeserializer : ScriptBundleDeserializerBase<ScriptBundle>
    {
        public ScriptBundleDeserializer(IUrlModifier urlModifier, TinyIoCContainer container)
            : base(urlModifier, container)
        {
        }

        protected override ScriptBundle CreateBundle(XElement element)
        {
            var scriptBundle = new ScriptBundle(GetPathAttribute());
            AssignScriptBundleProperties(scriptBundle, element);
            return scriptBundle;
        }
    }
}