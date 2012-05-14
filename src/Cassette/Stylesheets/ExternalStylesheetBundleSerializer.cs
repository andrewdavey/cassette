using System.Xml.Linq;

namespace Cassette.Stylesheets
{
    class ExternalStylesheetBundleSerializer : StylesheetBundleSerializerBase<ExternalStylesheetBundle>
    {
        public ExternalStylesheetBundleSerializer(XContainer container)
            : base(container)
        {
        }

        protected override XElement CreateElement()
        {
            var element = base.CreateElement();
            element.Add(new XAttribute("Url", Bundle.ExternalUrl));

            return element;
        }
    }
}