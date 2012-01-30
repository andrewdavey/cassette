using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.HtmlTemplates.Manifests;
using Cassette.Scripts.Manifests;
using Cassette.Stylesheets.Manifests;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleManifestSetWriter_Tests
    {
        [Fact]
        public void CanWriteBundleManifestsToStreamAndReadBackIntoEqualBundleManifests()
        {
            using (var stream = new MemoryStream())
            {
                var originalBundleManifests = OriginalBundleManifests();
                WriteBundleManifestsToStream(originalBundleManifests, stream);
                var readBundleManifests = BundleManifestsReadFromStream(stream);

                originalBundleManifests.SequenceEqual(readBundleManifests).ShouldBeTrue();
            }
        }

        BundleManifest[] OriginalBundleManifests()
        {
            return new BundleManifest[]
            {
                new ScriptBundleManifest { Path = "~/a", Hash = new byte[] { 1 } },
                new StylesheetBundleManifest { Path = "~/b", Hash = new byte[] { 2 } },
                new ExternalStylesheetBundleManifest { Path = "~/c", Hash = new byte[] { 3 }, Url = "http://example.com/stylesheet" },
                new ExternalScriptBundleManifest { Path = "~/d", Hash = new byte[] { 4 }, Url = "http://example.com/script" },
                new HtmlTemplateBundleManifest { Path = "~/e", Hash = new byte[] { 5 } }
            };
        }

        void WriteBundleManifestsToStream(IEnumerable<BundleManifest> originalBundleManifests, MemoryStream stream)
        {
            var writer = new BundleManifestSetWriter(stream);
            writer.Write(originalBundleManifests);
        }

        IEnumerable<BundleManifest> BundleManifestsReadFromStream(Stream stream)
        {
            stream.Position = 0;
            var reader = new BundleManifestSetReader(stream);
            return reader.Read();
        }
    }
}