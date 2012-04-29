using System.Xml.Linq;
using Cassette.IO;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class BundleManifestReader_Tests
    {
        readonly XElement bundleElement;
        readonly TestableBundleDeserializer deserializer;

        public BundleManifestReader_Tests()
        {
            bundleElement = new XElement(
                "Bundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", ""),
                new XElement("Html", "EXPECTED-HTML")
            );
            var directory = new FakeFileSystem();
            var urlModifier = new VirtualDirectoryPrepender("/");
            deserializer = new TestableBundleDeserializer(directory, urlModifier);
        }

        [Fact]
        public void BundleRenderReturnsHtmlFromElement()
        {
            deserializer.Deserialize(bundleElement).Render().ShouldEqual("EXPECTED-HTML");
        }

        class TestableBundleDeserializer : BundleDeserializer<TestableBundle>
        {
            public TestableBundleDeserializer(IDirectory directory, IUrlModifier urlModifier)
                : base(directory, urlModifier)
            {
            }

            protected override TestableBundle CreateBundle(XElement element)
            {
                return new TestableBundle(GetPathAttribute());
            }
        }
    }
}
