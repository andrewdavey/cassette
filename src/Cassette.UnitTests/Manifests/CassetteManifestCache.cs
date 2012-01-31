using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Cassette.IO;
using Cassette.Scripts.Manifests;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class CassetteManifestCache_Tests : IDisposable
    {
        readonly Mock<IFile> file;
        readonly CassetteManifestCache cache;
        readonly MemoryStream outputStream;

        public CassetteManifestCache_Tests()
        {
            file = new Mock<IFile>();
            cache = new CassetteManifestCache(file.Object);

            outputStream = new MemoryStream();
            file.Setup(f => f.Open(FileMode.Create, FileAccess.Write, FileShare.ReadWrite)).Returns(outputStream);
        }

        [Fact]
        public void ImplementsICassetteManifestCache()
        {
            cache.ShouldImplement<ICassetteManifestCache>();
        }

        [Fact]
        public void GivenFileDoesNotExist_WhenLoadCassetteManifest_ThenReturnEmptyCassetteManifest()
        {
            GivenFileDoesNotExist();
            var manifest = cache.LoadCassetteManifest();
            manifest.BundleManifests.Count.ShouldEqual(0);
        }

        [Fact]
        public void GivenFileDoesNotExist_WhenLoadCassetteManifest_ThenManifestLastWriteTimeUtcIsInThePast()
        {
            GivenFileDoesNotExist();
            var manifest = cache.LoadCassetteManifest();
            manifest.LastWriteTimeUtc.ShouldEqual(DateTime.MinValue);
        }

        [Fact]
        public void GivenFileContainsBundleXml_WhenLoadCassetteManifest_ThenManifestIsReadFromXml()
        {
            var xml = "<?xml version=\"1.0\"?><Cassette LastWriteTimeUtc=\"2012-01-01 00:00\" Version=\"VERSION\"><ScriptBundle Path=\"~\" Hash=\"\"/></Cassette>";
            GivenFileContains(xml);

            var manifest = cache.LoadCassetteManifest();

            manifest.LastWriteTimeUtc.ShouldEqual(new DateTime(2012, 1, 1, 0, 0, 0));
            manifest.Version.ShouldEqual("VERSION");
            manifest.BundleManifests.Count.ShouldEqual(1);
        }

        [Fact]
        public void GivenManifestWithBundle_WhenSaveCassetteManifest_ThenXmlContainsBundleElement()
        {
            var manifest = new CassetteManifest(
                "",
                new[] { new ScriptBundleManifest { Path = "~", Hash = new byte[0] } }
            );
            cache.SaveCassetteManifest(manifest);

            var xml = SavedXml();
            xml.Root.Elements("ScriptBundle").Count().ShouldEqual(1);
        }

        [Fact]
        public void GivenManifestWithBundle_WhenSaveCassetteManifest_ThenXmlLastWriteTimeUtcAttributeIsNow()
        {
            var now = DateTime.UtcNow;
            now = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Month, now.Second);

            var manifest = new CassetteManifest("", new BundleManifest[0]);
            cache.SaveCassetteManifest(manifest);

            var xml = SavedXml();
            var lastWriteTimeUtc = DateTime.Parse(xml.Root.Attribute("LastWriteTimeUtc").Value);
            (lastWriteTimeUtc >= now).ShouldBeTrue();
        }

        [Fact]
        public void GivenFileExists_WhenClear_ThenFileDeleted()
        {
            file.Setup(f => f.Exists).Returns(true);

            cache.Clear();

            file.Verify(f => f.Delete());
        }

        void GivenFileContains(string content)
        {
            file.Setup(f => f.Exists).Returns(true);
            file.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)).Returns(() => content.AsStream());
        }

        void GivenFileDoesNotExist()
        {
            file.Setup(f => f.Exists).Returns(false);
        }

        XDocument SavedXml()
        {
            var bytes = outputStream.ToArray();
            var xml = XDocument.Load(new MemoryStream(bytes));
            return xml;
        }

        void IDisposable.Dispose()
        {
            outputStream.Dispose();
        }
    }
}