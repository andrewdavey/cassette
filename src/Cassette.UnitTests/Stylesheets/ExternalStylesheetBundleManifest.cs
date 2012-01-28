using System.Xml.Linq;
using Cassette.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetBundleManifest_Tests
    {
        readonly ExternalStylesheetBundleManifest manifest;
        readonly ExternalStylesheetBundle createdBundle;

        public ExternalStylesheetBundleManifest_Tests()
        {
            manifest = new ExternalStylesheetBundleManifest
            {
                Path = "~",
                Hash = new byte[] { },
                Media = "MEDIA",
                Url = "http://example.com/"
            };
            createdBundle = (ExternalStylesheetBundle)manifest.CreateBundle(Mock.Of<IFile>());
        }

        [Fact]
        public void CreatedBundleMediaEqualsManifestMedia()
        {
            createdBundle.Media.ShouldEqual(manifest.Media);
        }

        [Fact]
        public void CreatedBundleUrlEqualsManifestUrl()
        {
            createdBundle.Url.ShouldEqual(manifest.Url);
        }
    }

    public class ExternalStylesheetBundleManifest_InitializeFromXElement_Tests
    {
        [Fact]
        public void UrlAssignedFromAttribute()
        {
            var manifest = new ExternalStylesheetBundleManifest();
            manifest.InitializeFromXElement(new XElement("ExternalStylesheetBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", ""),
                new XAttribute("Url", "http://example.com/")
            ));

            manifest.Url.ShouldEqual("http://example.com/");
        }

        [Fact]
        public void GivenUrlAttributeMissingThenExceptionThrown()
        {
            var manifest = new ExternalStylesheetBundleManifest();
            Assert.Throws<InvalidBundleManifestException>(
                () => manifest.InitializeFromXElement(new XElement("ExternalStylesheetBundle",
                    new XAttribute("Path", "~"),
                    new XAttribute("Hash", "")
                ))
            );
        }

        [Fact]
        public void MediaAssignedFromAttribute()
        {
            var manifest = new ExternalStylesheetBundleManifest();
            manifest.InitializeFromXElement(new XElement("ExternalStylesheetBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", ""),
                new XAttribute("Url", "http://example.com/"),
                new XAttribute("Media", "expected-media")
            ));

            manifest.Media.ShouldEqual("expected-media");
        }
    }
}
