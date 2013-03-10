using System.Xml.Linq;

namespace Cassette.HtmlTemplates
{
    class HtmlTemplateBundleSerializer : BundleSerializer<HtmlTemplateBundle>
    {
        public HtmlTemplateBundleSerializer(XContainer container) : base(container)
        {
        }

        protected override XElement CreateElement()
        {
            var element = base.CreateElement();
            AddRenderer(element);
            return element;
        }

        void AddRenderer(XElement element)
        {
            element.Add(new XAttribute("Renderer", Bundle.Renderer.GetType().AssemblyQualifiedName));
        }
    }
}