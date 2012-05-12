using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Cassette.IO;
using Cassette.Utilities;

namespace Cassette
{
    abstract class BundleDeserializer<T> : IBundleDeserializer<T>
        where T : Bundle
    {
        readonly IUrlModifier urlModifier;
        IDirectory directory;
        XElement element;

        protected BundleDeserializer(IUrlModifier urlModifier)
        {
            this.urlModifier = urlModifier;
        }

        protected abstract T CreateBundle(XElement element);

        public T Deserialize(XElement element, IDirectory cacheDirectory)
        {
            this.element = element;
            directory = cacheDirectory;
            var bundle = CreateBundle(element);
            var descriptorFilePath = GetOptionalAttribute("DescriptorFilePath");
            bundle.DescriptorFilePath = descriptorFilePath;
            bundle.Hash = GetHashAttribute();
            var contentType = GetOptionalAttribute("ContentType");
            if (contentType != null) bundle.ContentType = contentType;
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
            var assets = assetElements.Select(e => new AssetDeserializer().Deserialize(e)).ToArray();
            if (assets.Length == 0) return;
            var contentFile = directory.GetFile(bundle.CacheFilename);
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