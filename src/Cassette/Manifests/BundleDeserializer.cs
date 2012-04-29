using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette.Manifests
{
    abstract class BundleDeserializer<T> : IBundleDeserializer<T>
        where T : Bundle
    {
        readonly IDirectory directory;
        readonly IUrlModifier urlModifier;
        XElement element;

        protected BundleDeserializer(IDirectory directory, IUrlModifier urlModifier)
        {
            this.directory = directory;
            this.urlModifier = urlModifier;
        }

        protected abstract T CreateBundle(XElement element);

        public T Deserialize(XElement element)
        {
            this.element = element;
            var bundle = CreateBundle(element);
            bundle.Hash = GetHashAttribute();
            bundle.ContentType = GetOptionalAttribute("ContentType");
            bundle.PageLocation = GetOptionalAttribute("PageLocation");
            AddAssets(bundle);
            AddReferences(bundle);
            AddHtmlAttributes(bundle);
            return bundle;
        }

        protected string GetPathAttribute()
        {
            return GetRequiredAttribute("Path");
        }

        byte[] GetHashAttribute()
        {
            try
            {
                return ByteArrayExtensions.FromHexString(GetRequiredAttribute("Hash"));
            }
            catch (ArgumentException ex)
            {
                throw new CassetteDeserializationException("Bundle manifest element has invalid Hash attribute.", ex);
            }
            catch (FormatException ex)
            {
                throw new CassetteDeserializationException("Bundle manifest element has invalid Hash attribute.", ex);                
            }
        }

        string GetRequiredAttribute(string attributeName)
        {
            return element.AttributeValueOrThrow(
                attributeName,
                () => new CassetteDeserializationException(string.Format("Bundle manifest element missing \"{0}\" attribute.", attributeName))
                );
        }

        protected string GetOptionalAttribute(string attributeName)
        {
            return element.AttributeValueOrNull(attributeName);
        }

        string GetHtml()
        {
            var htmlElement = element.Elements("Html").FirstOrDefault();
            return htmlElement != null
                ? htmlElement.Value
                : "";
        }

        void AddAssets(Bundle bundle)
        {
            var assetElements = element.Elements("Asset");
            var assets = assetElements.Select(e => new AssetDeserializer().Deserialize(e));
            var contentFile = directory.GetFile(bundle.Path.Substring(2));
            bundle.Assets.Add(new CachedBundleContent(contentFile, assets, urlModifier));
        }

        void AddReferences(Bundle bundle)
        {
            var paths = GetReferencePaths();
            foreach (var path in paths)
            {
                bundle.AddReference(path);
            }
        }

        void AddHtmlAttributes(Bundle bundle)
        {
            var attributeElements = element.Elements("HtmlAttribute");
            foreach (var attributeElement in attributeElements)
            {
                AddHtmlAttribute(bundle, attributeElement);
            }
        }

        void AddHtmlAttribute(Bundle bundle, XElement attributeElement)
        {
            var name = GetHtmlAttributeElementNameAttribute(attributeElement);
            var value = attributeElement.AttributeValueOrNull("Value");
            bundle.HtmlAttributes.Add(name, value);
        }

        string GetHtmlAttributeElementNameAttribute(XElement attributeElement)
        {
            return attributeElement.AttributeValueOrThrow(
                "Name",
                () => new CassetteDeserializationException("HtmlAttribute manifest element is missing \"Name\" attribute.")
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
                () => new CassetteDeserializationException("Reference manifest element missing \"Path\" attribute.")
            );
        }

        protected ConstantHtmlRenderer<TBundle> CreateHtmlRenderer<TBundle>() where TBundle : Bundle
        {
            return new ConstantHtmlRenderer<TBundle>(GetHtml(), urlModifier);
        }
    }
}