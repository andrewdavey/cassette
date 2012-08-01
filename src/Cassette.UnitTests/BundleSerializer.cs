using System.Linq;
using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleSerializer_Tests
    {
        class TestableBundleSerializer : BundleSerializer<TestableBundle>
        {
            public TestableBundleSerializer(XContainer container) : base(container)
            {
            }
        }

        readonly TestableBundle bundle;
        XElement element;

        public BundleSerializer_Tests()
        {
            bundle = new TestableBundle("~")
            {
                Hash = new byte[] { 1, 2, 3 },
                ContentType = "content-type",
                PageLocation = "page-location",
                Assets =
                    {
                        new StubAsset("~/asset", "asset-content")
                        {
                            ReferenceList =
                                {
                                    new AssetReference("~/asset", "~/raw-file/reference", -1, AssetReferenceType.RawFilename)
                                }
                        }
                    },
                HtmlAttributes =
                    {
                        { "attribute", "value" }
                    }
            };
            bundle.AddReference("~/bundle-reference");

            SerializeToElement();
        }

        void SerializeToElement()
        {
            var container = new XDocument();
            var writer = new TestableBundleSerializer(container);
            writer.Serialize(bundle);
            element = container.Root;
        }

        [Fact]
        public void PathAttributeEqualsManifestPath()
        {
            element.Attribute("Path").Value.ShouldEqual(bundle.Path);
        }

        [Fact]
        public void HashAttributeEqualsHexStringOfManifestHash()
        {
            element.Attribute("Hash").Value.ShouldEqual("010203");
        }

        [Fact]
        public void ContentTypeAttributeEqualsManifestContentType()
        {
            element.Attribute("ContentType").Value.ShouldEqual(bundle.ContentType);
        }

        [Fact]
        public void PageLocationAttributeEqualsManifestPageLocation()
        {
            element.Attribute("PageLocation").Value.ShouldEqual(bundle.PageLocation);
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
            bundle.ContentType = null;
            SerializeToElement();
            element.Attribute("ContentType").ShouldBeNull();
        }

        [Fact]
        public void GivenPageLocationNullThenElementHasNoPageLocationAttribute()
        {
            bundle.PageLocation = null;
            SerializeToElement();
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
            bundle.HtmlAttributes.Clear();
            bundle.HtmlAttributes.Add("attribute", null);
            SerializeToElement();

            var htmlAttributeElement = element.Element("HtmlAttribute");
            htmlAttributeElement.Attribute("Name").Value.ShouldEqual("attribute");
            htmlAttributeElement.Attribute("Value").ShouldBeNull();
        }

        [Fact]
        public void ElementHasHtmlElementWithBundleManifestHtml()
        {
            bundle.RenderResult = "EXPECTED-HTML";
            SerializeToElement();
            element.Element("Html").Value.ShouldEqual("EXPECTED-HTML");
        }
    }
}