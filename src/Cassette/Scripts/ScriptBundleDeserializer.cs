using System.Xml.Linq;
using Cassette.IO;

namespace Cassette.Scripts
{
    class ScriptBundleDeserializer : ScriptBundleDeserializerBase<ScriptBundle>
    {
        public ScriptBundleDeserializer(IDirectory directory, IUrlModifier urlModifier)
            : base(directory, urlModifier)
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