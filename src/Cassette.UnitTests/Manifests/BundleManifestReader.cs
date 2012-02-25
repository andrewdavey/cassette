using System;
using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class BundleManifestReader_Tests
    {
        readonly XElement manifestElement;
        readonly TestableBundleManifestReader reader;

        public BundleManifestReader_Tests()
        {
            manifestElement = new XElement(
                "Bundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", "")
            );
            reader = new TestableBundleManifestReader(manifestElement);
        }

        [Fact]
        public void GivenXmlHasHtmlElement_ThenReadManifestHtmlEqualsElementContent()
        {
            manifestElement.Add(new XElement("Html", "EXPECTED-HTML"));
            reader.Read().Html.ShouldEqual("EXPECTED-HTML");
        }

        [Fact]
        public void GivenXmlHasNoHtmlElement_ThenReadManifestHtmlIsEmptyString()
        {
            reader.Read().Html.ShouldEqual("");
        }

        class TestableBundleManifestReader : BundleManifestReader<TestableBundleManifest>
        {
            public TestableBundleManifestReader(XElement element)
                : base(element)
            {
            }
        }

        class TestableBundleManifest : BundleManifest
        {
            protected override Bundle CreateBundleCore()
            {
                throw new NotImplementedException();
            }
        }
    }
}
