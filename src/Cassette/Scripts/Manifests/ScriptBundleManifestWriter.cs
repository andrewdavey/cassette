using System.Xml.Linq;

namespace Cassette.Scripts.Manifests
{
    class ScriptBundleSerializer : ScriptBundleSerializerBase<ScriptBundle>
    {
        public ScriptBundleSerializer(XContainer container) 
            : base(container)
        {
        }
    }
}