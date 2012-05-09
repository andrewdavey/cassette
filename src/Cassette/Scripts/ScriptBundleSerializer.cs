using System.Xml.Linq;

namespace Cassette.Scripts
{
    class ScriptBundleSerializer : ScriptBundleSerializerBase<ScriptBundle>
    {
        public ScriptBundleSerializer(XContainer container) 
            : base(container)
        {
        }
    }
}