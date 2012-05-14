using System.Xml.Linq;

namespace Cassette.Scripts
{
    class ScriptBundleDeserializer : ScriptBundleDeserializerBase<ScriptBundle>
    {
        public ScriptBundleDeserializer(IUrlModifier urlModifier)
            : base(urlModifier)
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