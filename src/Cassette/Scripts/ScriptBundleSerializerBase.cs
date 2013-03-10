using System.Xml.Linq;

namespace Cassette.Scripts
{
    abstract class ScriptBundleSerializerBase<T> : BundleSerializer<T>
        where T : ScriptBundle
    {
        XElement element;

        protected ScriptBundleSerializerBase(XContainer container)
            : base(container)
        {
        }

        protected override XElement CreateElement()
        {
            element = base.CreateElement();
            AddRenderer();
            AddConditionIfNotNull();

            return element;
        }

        void AddRenderer()
        {
            element.Add(new XAttribute("Renderer", Bundle.Renderer.GetType().AssemblyQualifiedName));
        }

        void AddConditionIfNotNull()
        {
            if (Bundle.Condition != null)
            {
                element.Add(new XAttribute("Condition", Bundle.Condition));
            }
        }
    }
}