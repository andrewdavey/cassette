using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette
{
    abstract class BundleSerializer<T> where T : Bundle
    {
        readonly XContainer container;
        T bundle;

        protected BundleSerializer(XContainer container)
        {
            this.container = container;
        }

        public void Serialize(T bundle)
        {
            this.bundle = bundle;
            var element = CreateElement();
            container.Add(element);
        }

        protected virtual XElement CreateElement()
        {
            var element = new XElement(
                ConventionalXElementName(),
                PathAttribute(),
                HashAttribute(),
                ContentTypeAttribute(),
                PageLocationAttribute(),
                bundle.References.Select(SerializeReference),
                HtmlAttributeElements(),
                HtmlElement()
            );

            WriteAssetElements(element);

            return element;
        }

        protected T Bundle
        {
            get { return bundle; }
        }

        string ConventionalXElementName()
        {
            return bundle.GetType().Name;
        }

        XAttribute PathAttribute()
        {
            return new XAttribute("Path", bundle.Path);
        }

        XAttribute HashAttribute()
        {
            return new XAttribute("Hash", bundle.Hash.ToHexString());
        }

        XAttribute ContentTypeAttribute()
        {
            return bundle.ContentType != null ? new XAttribute("ContentType", bundle.ContentType) : null;
        }

        XAttribute PageLocationAttribute()
        {
            return bundle.PageLocation != null ? new XAttribute("PageLocation", bundle.PageLocation) : null;
        }

        IEnumerable<XElement> HtmlAttributeElements()
        {
            return from attribute in bundle.HtmlAttributes
                   select new XElement(
                       "HtmlAttribute",
                       new XAttribute("Name", attribute.Key),
                       attribute.Value != null ? new XAttribute("Value", attribute.Value) : null
                   );
        }

        XElement HtmlElement()
        {
            return new XElement("Html", bundle.Render());
        }

        void WriteAssetElements(XElement bundleElement)
        {
            var assetSerializer = new AssetSerializer(bundleElement);
            foreach (var asset in bundle.Assets)
            {
                assetSerializer.Serialize(asset);
            }
        }

        XElement SerializeReference(string path)
        {
            return new XElement(
                "Reference",
                new XAttribute("Path", path)
            );
        }
    }
}