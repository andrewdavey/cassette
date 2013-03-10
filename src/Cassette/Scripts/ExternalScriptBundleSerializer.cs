using System.Xml.Linq;

namespace Cassette.Scripts
{
    class ExternalScriptBundleSerializer : ScriptBundleSerializerBase<ExternalScriptBundle>
    {
        XElement element;

        public ExternalScriptBundleSerializer(XContainer container) : base(container)
        {
        }

        protected override XElement CreateElement()
        {
            element = base.CreateElement();

            AddUrlAttribute();
            AddFallbackConditionIfNotNull();
            AddFallbackRenderer();

            return element;
        }

        void AddUrlAttribute()
        {
            element.Add(new XAttribute("Url", Bundle.ExternalUrl));
        }

        void AddFallbackConditionIfNotNull()
        {
            if (Bundle.FallbackCondition != null)
            {
                element.Add(new XAttribute("FallbackCondition", Bundle.FallbackCondition));
            }
        }

        void AddFallbackRenderer()
        {
            element.Add(new XAttribute("FallbackRenderer", Bundle.FallbackRenderer.GetType().AssemblyQualifiedName));
        }
    }
}