using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Cassette.IO;
using Cassette.Scripts;
using Moq;
using Should;
using Xunit;

namespace Cassette.Caching
{
    public class BundleCollectionCache_Read_Tests
    {
        readonly Dictionary<string, IBundleDeserializer<Bundle>> deserializers;
        readonly Mock<IBundleDeserializer<ScriptBundle>> scriptBundleDeserializer;

        public BundleCollectionCache_Read_Tests()
        {
            scriptBundleDeserializer = new Mock<IBundleDeserializer<ScriptBundle>>();
            scriptBundleDeserializer
                .Setup(d => d.Deserialize(It.IsAny<XElement>(), It.IsAny<IDirectory>()))
                .Returns(new ScriptBundle("~"))
                .Verifiable();

            deserializers = new Dictionary<string, IBundleDeserializer<Bundle>>
            {
                { "ScriptBundle", scriptBundleDeserializer.Object }
            };
        }

        [Fact]
        public void GivenEmptyDirectory_ThenCacheReadFails()
        {
            using (var path = new TempDirectory())
            {
                var directory = new FileSystemDirectory(path);
                var cache = new BundleCollectionCache(directory, b => deserializers[b]);
                var result = cache.Read();
                result.IsSuccess.ShouldBeFalse();
            }
        }

        [Fact]
        public void GivenDirectoryWithManifest_ThenCacheReadSucceeds()
        {
            using (var path = new TempDirectory())
            {
                File.WriteAllText(
                    Path.Combine(path, "manifest.xml"),
                    "<?xml version=\"1.0\"?><BundleCollection Version=\"1\"></BundleCollection>"
                );

                var directory = new FileSystemDirectory(path);
                var cache = new BundleCollectionCache(directory, b => deserializers[b]);
                var result = cache.Read();
                result.IsSuccess.ShouldBeTrue();
            }
        }

        [Fact]
        public void CacheCreationDateEqualsManifestFileLastWriteTimeUtc()
        {
            using (var path = new TempDirectory())
            {
                var manifestFilename = Path.Combine(path, "manifest.xml");
                File.WriteAllText(
                    manifestFilename,
                    "<?xml version=\"1.0\"?><BundleCollection Version=\"1\"></BundleCollection>"
                );

                var directory = new FileSystemDirectory(path);
                var cache = new BundleCollectionCache(directory, b => deserializers[b]);
                var result = cache.Read();
                result.CacheCreationDate.ShouldEqual(File.GetLastWriteTimeUtc(manifestFilename));
            }
        }

        [Fact]
        public void GivenManifestAndBundleContentInDirectory_ThenCacheReadContainsBundle()
        {
            using (var path = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(path, "script/test"));
                File.WriteAllText(
                    Path.Combine(path, "manifest.xml"),
                    "<?xml version=\"1.0\"?>" +
                    "<BundleCollection Version=\"1\">" +
                    "<ScriptBundle Path=\"~/test\" Hash=\"010203\"/>" +
                    "</BundleCollection>"
                );
                File.WriteAllText(Path.Combine(path, "script/test/010203.js"), "");

                var directory = new FileSystemDirectory(path);
                var cache = new BundleCollectionCache(directory, b => deserializers[b]);
                cache.Read();

                scriptBundleDeserializer.VerifyAll();
            }
        }
    }
}