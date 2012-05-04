using System.IO;
using Cassette.IO;
using Should;
using Xunit;

namespace Cassette.Caching
{
    public class BundleCollectionCache_Clear_Tests
    {
        [Fact]
        public void ItDeletesAllFilesInCacheDirectory()
        {
            using (var path = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(path, "manifest.xml"), "");
                File.WriteAllText(Path.Combine(path, "010203.js"), "");
                File.WriteAllText(Path.Combine(path, "040506.css"), "");

                var directory = new FileSystemDirectory(path);

                var cache = new BundleCollectionCache(directory, b => null);
                cache.Clear();
                
                Directory.GetFiles(path).Length.ShouldEqual(0);
            }
        }
    }
}