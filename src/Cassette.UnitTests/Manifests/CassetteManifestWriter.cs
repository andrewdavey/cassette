using System;
using System.IO;
using Cassette.HtmlTemplates.Manifests;
using Cassette.Scripts.Manifests;
using Cassette.Stylesheets.Manifests;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class GivenManifestWrittenToStream_WhenReadStreamIntoNewManifest
    {
        readonly CassetteManifest originalManifest;
        readonly CassetteManifest newManifest;
        readonly DateTime startTime;

        public GivenManifestWrittenToStream_WhenReadStreamIntoNewManifest()
        {
            startTime = UtcNowToTheSecond();
            using (var stream = new MemoryStream())
            {
                originalManifest = CreateOriginalManifest();

                WriteManifestToStream(originalManifest, stream);
                newManifest = ReadManifestFromStream(stream);
            }
        }

        [Fact]
        public void NewManifestVersionEqualsOriginalManifestVersion()
        {
            newManifest.Version.ShouldEqual(originalManifest.Version);            
        }

        [Fact]
        public void NewManifestBundleManifestsEqualOriginalBundleManifests()
        {
            newManifest.BundleManifests.ShouldEqual(originalManifest.BundleManifests);
        }

        [Fact]
        public void NewManifestLastWriteTimeUtcIsJustAfterTestStartTime()
        {
            (newManifest.LastWriteTimeUtc >= startTime).ShouldBeTrue();
        }

        DateTime UtcNowToTheSecond()
        {
            var now = DateTime.UtcNow;
            return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
        }

        CassetteManifest CreateOriginalManifest()
        {
            return new CassetteManifest
            {
                Version = "VERSION",
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