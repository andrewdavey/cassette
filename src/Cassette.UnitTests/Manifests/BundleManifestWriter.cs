using System;
using System.Linq;
using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class BundleManifestWriter_Tests
    {
        readonly TestableBundleManifest manifest;
        XElement element;

        class TestableBundleManifest : BundleManifest
        {
            protected override Bundle CreateBundleCore()
            {
                throw new NotImplementedException();
            }
        }

        public BundleManifestWriter_Tests()
        {
            manifest = new TestableBundleManifest
            {
                Path = "~",
                Hash = new byte[] { 1, 2, 3 },
                ContentType = "content-type",
                PageLocation = "page-location",
                Assets =
                    {
                        new AssetManifest
                        {
                            Path = "~/asset",
                            References =
                                {
                                    new AssetReferenceManifest
                                    {
                                        Path = "~/raw-file/reference",
                                        Type = AssetReferenceType.RawFilename
                                    }
                                }
                        }
                    },
                References =
                    {
                        "~/bundle-reference"
                    },
                HtmlAttributes =
                    {
                        { "attribute", "value" }
                    },
                Content = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }
            };

            WriteToElement();
        }

        void WriteToElement()
        {
            var container = new XDocument();
            var writer = new BundleManifestWriter<TestableBundleManifest>(container);
            writer.Write(manifest);
            element = container.Root;
        }

        [Fact]
        public void PathAttributeEqualsManifestPath()
        {
            element.Attribute("Path").Value.ShouldEqual(manifest.Path);
        }

        [Fact]
        public void HashAttributeEqualsHexStringOfManifestHash()
        {
            element.Attribute("Hash").Value.ShouldEqual("010203");
        }

        [Fact]
        public void ContentTypeAttributeEqualsManifestContentType()
        {
            element.Attribute("ContentType").Value.ShouldEqual(manifest.ContentType);
        }

        [Fact]
        public void PageLocationAttributeEqualsManifestPageLocation()
        {
            element.Attribute("PageLocation").Value.ShouldEqual(manifest.PageLocation);
        }

        [Fact]
        public void ElementHasAssetChildElement()
        {
            element.Elements("Asset").Count().ShouldEqual(1);
        }

        [Fact]
        public void ElementHasReferenceChildElement()
        {
            element.Elements("Reference").Count().ShouldEqual(1);
        }

        [Fact]
        public void GivenContentTypeNullThenElementHasNoContentTypeAttribute()
        {
            manifest.ContentType = null;
            WriteToElement();
            element.Attribute("ContentType").ShouldBeNull();
        }

        [Fact]
        public void GivenPageLocationNullThenElementHasNoPageLocationAttribute()
        {
            manifest.PageLocation = null;
            WriteToElement();
            element.Attribute("PageLocation").ShouldBeNull();
        }

        [Fact]
        public void ElementHasHtmlAttributeElements()
        {
            var htmlAttributeElement = element.Element("HtmlAttribute");
            htmlAttributeElement.Attribute("Name").Value.ShouldEqual("attribute");
            htmlAttributeElement.Attribute("Value").Value.ShouldEqual("value");
        }

        [Fact]
        public void GivenHtmlAttributeWithNullValue_ThenHtmlAttributeElementHasOnlyTheNameAttribute()
        {
            manifest.HtmlAttributes.Clear();
            manifest.HtmlAttributes.Add("attribute", null);
            WriteToElement();

            var htmlAttributeElement = element.Element("HtmlAttribute");
            htmlAttributeElement.Attribute("Name").Value.ShouldEqual("attribute");
            htmlAttributeElement.Attribute("Value").ShouldBeNull();
        }

        [Fact]
        public void ElementHasContentElementWithBase64EncodedContent()
        {
            element.Element("Content").Value.ShouldEqual(Convert.ToBase64String(manifest.Content));
        }

        [Fact]
        public void GivenContentIsNullThenElementHasNoContentElement()
        {
            manifest.Content = null;
            WriteToElement();
            element.Elements("Content").ShouldBeEmpty();
        }

        [Fact]
        public void ElementHasHtmlElementWithBundleManifestHtml()
        {
            manifest.Html = "EXPECTED-HTML";
            WriteToElement();
            element.Element("Html").Value.ShouldEqual("EXPECTED-HTML");
        }
    }
}