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
                Directory.CreateDirectory(Path.Combine(path, "script/test1"));
                Directory.CreateDirectory(Path.Combine(path, "stylesheet/test2"));
                File.WriteAllText(Path.Combine(path, "manifest.xml"), "");
                File.WriteAllText(Path.Combine(path, "script/test1/010203.js"), "");
                File.WriteAllText(Path.Combine(path, "stylesheet/test2/040506.css"), "");

                var directory = new FileSystemDirectory(path);

                var cache = new BundleCollectionCache(directory, b => null);
                cache.Clear();
                
                Directory.GetFiles(path).Length.ShouldEqual(0);
                Directory.GetDirectories(path).Length.ShouldEqual(0);
            }
        }
    }
}