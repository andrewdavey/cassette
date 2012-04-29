using System.Xml.Linq;
using Cassette.Manifests;

namespace Cassette.Scripts.Manifests
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
            AddConditionIfNotNull();

            return element;
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