using System.IO;
using Cassette.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets.Manifests
{
    public class StylesheetBundleManifest_Tests
    {
        readonly StylesheetBundleManifest manifest;
        readonly StylesheetBundle createdBundle;

        public StylesheetBundleManifest_Tests()
        {
            manifest = new StylesheetBundleManifest
            {
                Path = "~",
                Hash = new byte[0],
                Media = "MEDIA"
            };
            var file = new Mock<IFile>();
            file.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                .Returns(() => "".AsStream());
            createdBundle = (StylesheetBundle)manifest.CreateBundle(file.Object);
        }

        [Fact]
        public void CreatedBundleMediaEqualsManifestMedia()
        {
            createdBundle.Media.ShouldEqual(manifest.Media);
        }
    }
}
