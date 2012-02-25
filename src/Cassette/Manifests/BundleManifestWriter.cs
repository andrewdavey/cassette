using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette.Manifests
{
    class BundleManifestWriter<T> where T : BundleManifest
    {
        readonly XContainer container;
        T manifest;

        public BundleManifestWriter(XContainer container)
        {
            this.container = container;
        }

        public void Write(T bundleManifest)
        {
            manifest = bundleManifest;
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
                manifest.References.Select(SerializeReference),
                HtmlAttributeElements(),
                ContentElement(),
                HtmlElement()
            );

            WriteAssetManifestElements(element);

            return element;
        }

        protected T Manifest
        {
            get { return manifest; }
        }

        string ConventionalXElementName()
        {
            var name = manifest.GetType().Name;
            return name.Substring(0, name.Length - "Manifest".Length);
        }

        XAttribute PathAttribute()
        {
            return new XAttribute("Path", manifest.Path);
        }

        XAttribute HashAttribute()
        {
            return new XAttribute("Hash", manifest.Hash.ToHexString());
        }

        XAttribute ContentTypeAttribute()
        {
            return manifest.ContentType != null ? new XAttribute("ContentType", manifest.ContentType) : null;
        }

        XAttribute PageLocationAttribute()
        {
            return manifest.PageLocation != null ? new XAttribute("PageLocation", manifest.PageLocation) : null;
        }

        IEnumerable<XElement> HtmlAttributeElements()
        {
            return from attribute in manifest.HtmlAttributes
                   select new XElement(
                       "HtmlAttribute",
                       new XAttribute("Name", attribute.Key),
                       attribute.Value != null ? new XAttribute("Value", attribute.Value) : null
                   );
        }

        XElement ContentElement()
        {
            return manifest.Content == null
                ? null
                : new XElement("Content", Convert.ToBase64String(manifest.Content));
        }

        XElement HtmlElement()
        {
            return (manifest.Html == null)
                ? null
                : new XElement("Html", manifest.Html());
        }

        void WriteAssetManifestElements(XElement element)
        {
            foreach (var assetManifest in manifest.Assets)
            {
                new AssetManifestWriter(element).Write(assetManifest);
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