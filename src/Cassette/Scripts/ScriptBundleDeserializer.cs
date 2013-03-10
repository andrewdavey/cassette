using System.Xml.Linq;
using Cassette.TinyIoC;

namespace Cassette.Scripts
{
    class ScriptBundleDeserializer : ScriptBundleDeserializerBase<ScriptBundle>
    {
        public ScriptBundleDeserializer(TinyIoCContainer container)
            : base(container)
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