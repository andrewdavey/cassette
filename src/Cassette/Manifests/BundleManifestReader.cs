using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cassette.Utilities;

namespace Cassette.Manifests
{
    interface IBundleManifestReader<out T> where T : BundleManifest
    {
        T Read();
    }

    abstract class BundleManifestReader<T> : IBundleManifestReader<T>
        where T : BundleManifest, new()
    {
        readonly XElement element;

        protected BundleManifestReader(XElement element)
        {
            this.element = element;
        }

        public T Read()
        {
            var manifest = new T
            {
                Path = GetRequiredAttribute("Path"),
                Hash = GetHashAttribute(),
                ContentType = GetOptionalAttribute("ContentType"),
                PageLocation = GetOptionalAttribute("PageLocation"),
                Content = GetContent(),
                Html = GetHtml
            };
            AddAssets(manifest);
            AddReferences(manifest);
            AddHtmlAttributes(manifest);
            InitializeBundleManifest(manifest, element);
            return manifest;
        }

        protected virtual void InitializeBundleManifest(T manifest, XElement manifestElement)
        {
            // Sub-classes may override this method to initialize their custom manifest properties.
        }

        byte[] GetHashAttribute()
        {
            try
            {
                return ByteArrayExtensions.FromHexString(GetRequiredAttribute("Hash"));
            }
            catch (ArgumentException ex)
            {
                throw new InvalidCassetteManifestException("Bundle manifest element has invalid Hash attribute.", ex);
            }
            catch (FormatException ex)
            {
                throw new InvalidCassetteManifestException("Bundle manifest element has invalid Hash attribute.", ex);                
            }
        }

        string GetRequiredAttribute(string attributeName)
        {
            return element.AttributeValueOrThrow(
                attributeName,
                () => new InvalidCassetteManifestException(string.Format("Bundle manifest element missing \"{0}\" attribute.", attributeName))
                );
        }

        string GetOptionalAttribute(string attributeName)
        {
            return element.AttributeValueOrNull(attributeName);
        }

        byte[] GetContent()
        {
            var contentElement = element.Elements("Content").FirstOrDefault();
            return contentElement != null
                ? Convert.FromBase64String(contentElement.Value)
                : null;
        }

        string GetHtml()
        {
            var htmlElement = element.Elements("Html").FirstOrDefault();
            return htmlElement != null
                ? htmlElement.Value
                : "";
        }

        void AddAssets(BundleManifest manifest)
        {
            var assetElements = element.Elements("Asset");
            var assetManifests = assetElements.Select(e => new AssetManifestReader(e).Read());
            foreach (var assetManifest in assetManifests)
            {
                manifest.Assets.Add(assetManifest);
            }
        }

        void AddReferences(BundleManifest manifest)
        {
            var paths = GetReferencePaths();
            foreach (var path in paths)
            {
                manifest.References.Add(path);
            }
        }

        void AddHtmlAttributes(BundleManifest manifest)
        {
            var attributeElements = element.Elements("HtmlAttribute");
            foreach (var attributeElement in attributeElements)
            {
                AddHtmlAttribute(manifest, attributeElement);
            }
        }

        void AddHtmlAttribute(BundleManifest manifest, XElement attributeElement)
        {
            var name = GetHtmlAttributeElementNameAttribute(attributeElement);
            var value = attributeElement.AttributeValueOrNull("Value");
            manifest.HtmlAttributes.Add(name, value);
        }

        string GetHtmlAttributeElementNameAttribute(XElement attributeElement)
        {
            return attributeElement.AttributeValueOrThrow(
                "Name",
                () => new InvalidCassetteManifestException("HtmlAttribute manifest element is missing \"Name\" attribute.")
            );
        }

        IEnumerable<string> GetReferencePaths()
        {
            var referenceElements = element.Elements("Reference");
            return referenceElements.Select(GetReferencePathAttribute);
        }

        string GetReferencePathAttribute(XElement referenceElement)
        {
            return referenceElement.AttributeValueOrThrow(
                "Path",
                () => new InvalidCassetteManifestException("Reference manifest element missing \"Path\" attribute.")
            );
        }
    }
}