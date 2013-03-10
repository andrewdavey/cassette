using System.Xml.Linq;

namespace Cassette.Stylesheets
{
    class StylesheetBundleSerializerBase<T> : BundleSerializer<T>
        where T : StylesheetBundle
    {
        protected StylesheetBundleSerializerBase(XContainer container)
            : base(container)
        {
        }

        protected override XElement CreateElement()
        {
            var element = base.CreateElement();
            AddRenderer(element);
            AddConditionIfNotNull(element);
            return element;
        }

        void AddRenderer(XElement element)
        {
            element.Add(new XAttribute("Renderer", Bundle.Renderer.GetType().AssemblyQualifiedName));
        }

        void AddConditionIfNotNull(XElement element)
        {
            if (Bundle.Condition != null)
            {
                element.Add(new XAttribute("Condition", Bundle.Condition));
            }
        }
    }
}