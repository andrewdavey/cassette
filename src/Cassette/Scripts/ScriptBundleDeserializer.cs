using System.Xml.Linq;

namespace Cassette.Scripts
{
    class ScriptBundleDeserializer : ScriptBundleDeserializerBase<ScriptBundle>
    {
        public ScriptBundleDeserializer(IUrlModifier urlModifier, IApplicationRootPrepender applicationRootPrepender)
            : base(urlModifier, applicationRootPrepender)
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