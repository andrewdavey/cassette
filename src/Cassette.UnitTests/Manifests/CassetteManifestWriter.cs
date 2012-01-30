using System.IO;
using Cassette.HtmlTemplates.Manifests;
using Cassette.Scripts.Manifests;
using Cassette.Stylesheets.Manifests;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class BundleManifestSetWriter_Tests
    {
        [Fact]
        public void CanWriteBundleManifestsToStreamAndReadBackIntoEqualBundleManifests()
        {
            using (var stream = new MemoryStream())
            {
                var originalManifest = CreateOriginalManifest();

                WriteManifestToStream(originalManifest, stream);
                var newManifests = ReadManifestFromStream(stream);

                originalManifest.ShouldEqual(newManifests);
            }
        }

        CassetteManifest CreateOriginalManifest()
        {
            return new CassetteManifest
            {
                BundleManifests =
                {
                    new ScriptBundleManifest { Path = "~/a", Hash = new byte[] { 1 } },
                    new StylesheetBundleManifest { Path = "~/b", Hash = new byte[] { 2 } },
                    new ExternalStylesheetBundleManifest { Path = "~/c", Hash = new byte[] { 3 }, Url = "http://example.com/stylesheet" },
                    new ExternalScriptBundleManifest { Path = "~/d", Hash = new byte[] { 4 }, Url = "http://example.com/script" },
                    new HtmlTemplateBundleManifest { Path = "~/e", Hash = new byte[] { 5 } }
                }
            };
        }

        void WriteManifestToStream(CassetteManifest manifest, Stream stream)
        {
            var writer = new CassetteManifestWriter(stream);
            writer.Write(manifest);
        }

        CassetteManifest ReadManifestFromStream(Stream stream)
        {
            stream.Position = 0;
            var reader = new CassetteManifestReader(stream);
            return reader.Read();
        }
    }
}