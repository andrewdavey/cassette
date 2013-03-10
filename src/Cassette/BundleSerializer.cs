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
                DescriptorFilePathAttribute(),
                PathAttribute(),
                HashAttribute(),
                ContentTypeAttribute(),
                PageLocationAttribute(),
                bundle.References.Select(SerializeReference),
                HtmlAttributeElements()
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

        XAttribute DescriptorFilePathAttribute()
        {
            return bundle.IsFromDescriptorFile 
                ? new XAttribute("DescriptorFilePath", bundle.DescriptorFilePath)
                : null;
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

        void WriteAssetElements(XElement bundleElement)
        {
            var assetSerializer = new AssetSerializer(bundleElement);
            bundle.Accept(assetSerializer);
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